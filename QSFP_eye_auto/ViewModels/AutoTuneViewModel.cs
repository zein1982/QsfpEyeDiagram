using QSFP_eye_auto.Classes;
using QSFP_eye_auto.Models;
using QsfpEyeDiagram.ViewModels;
using QsfpEyeDiagram.ViewModels.Commands;
using Std.Modules.ConfigurationParameters.Qsfp;
using Std.Equipment.Anritsu.Bertwave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;
using System.IO;

namespace QSFP_eye_auto.ViewModels
{

    internal class AutoTuneViewModel : ViewModelBase
    {
        //private readonly double attenuation = 3.95;

        public readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        public QsfpEyeDiagramViewModel QsfpViewModel { get; set; } = new QsfpEyeDiagramViewModel(true);
        //public LocalViewModel LocalVM { get; set; } = new LocalViewModel();

        public ObservableCollection<string> Log { get; set; } = new ObservableCollection<string>();

        public Dictionary<(int b, int m), double> data = new Dictionary<(int, int), double>();

        #region Поля
        private List<int> _chList = new List<int> { 1, 2, 3, 4 };
        public List<int> ChList
        {
            get => _chList;
        }
        private int _selectedChannel = 1;
        public int SelectedChannel
        {
            get => _selectedChannel;
            set => Set(ref _selectedChannel, value);
        }

        private int _firstBias = 260;
        public int FirstBias
        {
            get => _firstBias;
            set=> Set(ref _firstBias, value);
        }

        private int _firstMod = 680;
        public int FirstMod 
        { 
            get => _firstMod;  
            set=>Set(ref _firstMod, value);
        }

        private List<TuneLimits> _limits = new List<TuneLimits>();
        public List<TuneLimits> Limits
        {
            get => _limits;
            set => Set(ref _limits, value);
        }

        private string _sbText = "Нет действий";
        public string SbText
        {
            get => _sbText;
            set => Set(ref _sbText, value);
        }

        private BitmapImage _oscScreen = new BitmapImage();
        public BitmapImage OscScreen
        {
            get => _oscScreen;
            set => Set(ref _oscScreen, value);
        }

        private int _maxHops = 8;
        public int MaxHops { get => _maxHops; set => Set(ref _maxHops, value); }

        #endregion

        #region Constructors
        public AutoTuneViewModel()
        {
                //_dispatcher = Dispatcher.CurrentDispatcher;
                //SbText = "Загрузка диапазонов настройки";
                using (DContext db = new DContext())
                {
                    Limits.AddRange(db.QsfpLimits);
                }
                //SbText = "Диапазоны настроек загружены";
                Trace.WriteLine("Новый AutoTuneViewModel");
        }


        #endregion

        #region Команды

        private ICommand _tuneCommand;
        public ICommand TuneCommand
        {
            get => _tuneCommand ?? (_tuneCommand = new RelayCommand(
                p => true,
                p => Task.Run(async () => 
                {
                    await Tune(); 
                })
                ));
        }

        private async Task Tune()
        {
            Dictionary<string, double> EyeParams = new Dictionary<string, double>();

            _ = MyMessage("Начало настройки");
            QsfpViewModel.MState = ModuleState.NowTune;
            QsfpViewModel.IsOperationInProgress = true;
            await Task.Run(() => QsfpViewModel._model.RefreshUtbStatus());

            //await QsfpViewModel.ReadCdrData();
            //Подключение к приборам
            _ = MyMessage("Подключение к OSA");
            await QsfpViewModel.ConnectToOsa(Properties.Settings.Default.OsaIp);
            if (!QsfpViewModel.IsOsaConnected)
            {
                _ = MyMessage("Не удалось подключиться к OSA");
                return;
            }
            _ = MyMessage("Получение данных от OSA");
            await Task.Run(() => QsfpViewModel.RefreshOsaData());
            //_ = MyMessage("Авто-настройка TEC");
            //await QsfpViewModel.AutoTuneTec2();


            _ = MyMessage("Подключение к осциллографу");
            await Task.Run(() => QsfpViewModel.ConnectToBertwave(Properties.Settings.Default.BertIP));
            if(!QsfpViewModel.IsBertwaveConnected)
            {
                _ = MyMessage("Не удалось подключиться к осциллографу");
                return;
            }
            _ = MyMessage("Настрйка каналов");
            QsfpViewModel.logger.Trace("Настрйка каналов");
            //foreach (Channel ch in Enum.GetValues(typeof(Channel)))
            //{
            
                await NewTune((Channel)SelectedChannel/*ch*/);
            //}


            if (CheckAllBertParams((Channel)SelectedChannel/*Channel.Second*/)/* + CheckAllBertParams(Channel.First) + CheckAllBertParams(Channel.Zero) + CheckAllBertParams(Channel.Third)*/ == 0)
                QsfpViewModel.MState = ModuleState.Tuned;
            else
                QsfpViewModel.MState = ModuleState.NotTuned;

            _ = MyMessage("Настройка модуля завершена");
            QsfpViewModel.IsOperationInProgress = false;
            _ = Task.Run(() => QsfpViewModel.DisconnectFromBertwave());
        }
        #endregion

        private BertwaveChannels GetBertChannel(Channel ch)
        {
            switch (ch)
            {
                case Channel.First: return BertwaveChannels.B;
                case Channel.Second: return BertwaveChannels.C;
                case Channel.Third: return BertwaveChannels.D;
                case Channel.Zero:
                default: return BertwaveChannels.A;
            }
        }

        private double GetEr(Channel ch)
        {
            BertwaveChannels bch = GetBertChannel(ch);
            QsfpViewModel._model.Bertwave.GetBertWaveER(bch, out double result);
            return result;
        }

        private double GetSnr(Channel ch)
        {
            QsfpViewModel._model.Bertwave.GetBertWaveSNR(GetBertChannel(ch), out double result);
            return result;
        }

        private int CheckAllBertParams(Channel ch)
        {
            int result = 0;

            double ap/* = -10.0*/;
            //double cr;
            double er/* = 0*/;
            double snr/* = 0*/;
            //double jpp = 0;
            double mm/* = 0*/;
            QsfpViewModel.PutDataToParameters(QsfpViewModel._model.BertWaveParameters);
            switch(ch)
            {
                case Channel.Zero:
                    ap = QsfpViewModel.AvePowerA;
                    //cr = QsfpViewModel.CrossA;
                    er = QsfpViewModel.ExtRatioA;//>=4.3;
                    snr = QsfpViewModel.SnrA;// >=5;
                    //jpp = QsfpViewModel.JittPpA;
                    mm = QsfpViewModel.MaskMarginA;//>=10;
                    break;
                case Channel.First:
                    ap = QsfpViewModel.AvePowerB;
                    //cr = QsfpViewModel.CrossB;
                    er = QsfpViewModel.ExtRatioB;//>=4.3;
                    snr = QsfpViewModel.SnrB;// >=5;
                    //jpp = QsfpViewModel.JittPpB;
                    mm = QsfpViewModel.MaskMarginB;//>=10;
                    break;
                case Channel.Second:
                    ap = QsfpViewModel.AvePowerC;
                    //cr = QsfpViewModel.CrossC;
                    er = QsfpViewModel.ExtRatioC;//>=4.3;
                    snr = QsfpViewModel.SnrC;// >=5;
                    //jpp = QsfpViewModel.JittPpC;
                    mm = QsfpViewModel.MaskMarginC;//>=10;
                    break;
                case Channel.Third:
                    ap = QsfpViewModel.AvePowerD;
                    //cr = QsfpViewModel.CrossD;
                    er = QsfpViewModel.ExtRatioD;//>=4.3;
                    snr = QsfpViewModel.SnrD;// >=5;
                    //jpp = QsfpViewModel.JittPpD;
                    mm = QsfpViewModel.MaskMarginD;//>=10;
                    break;
                default:
                    ap = -10.0;
                    //cr=
                    er = 0;
                    snr = 0;
                    //jpp = 0;
                    mm = 0;
                    break;
            }

            if (ap > 0 && ap < -5) { result += 1; }
            //if(cr){}
            if (er < 4.3) { result += 2; }
            if(snr < 5.0) { result += 4; }
            //if (jpp > 15) { result += 8 }
            //if (mm < 0.0) { result += 8; }
            return result;
        }

        private double GetMM(Channel ch)
        {
            BertwaveChannels bch = GetBertChannel(ch);
            QsfpViewModel._model.Bertwave.GetBertWaveMM(bch, out double result);
            return result;
        }

        private async Task TuneMM(Channel ch)
        {
            double mm = GetMM(ch);
            int old_eq = 0;
            switch (ch)
            {
                case Channel.Zero:
                    old_eq = QsfpViewModel.ZeroChannel.EqualizerPhaseWithMagnitude;
                    break;
                case Channel.First:
                    old_eq = QsfpViewModel.FirstChannel.EqualizerPhaseWithMagnitude;
                    break;
                case Channel.Second:
                    old_eq = QsfpViewModel.SecondChannel.EqualizerPhaseWithMagnitude;
                    break;
                case Channel.Third:
                    old_eq = QsfpViewModel.ThirdChannel.EqualizerPhaseWithMagnitude;
                    break;
            }
            if (mm >= 10)
            {
                _ = MyMessage("MaskMargin в норме");
                return;
            }
            int eq = (int)Limits[(int)ch].Min_eq;
            await QsfpViewModel.WriteEqualizer(eq, ch);
            QsfpViewModel._model.Bertwave.DisplayAutoScale();
            int de = (int)(Limits[(int)ch].Max_eq - Limits[(int)ch].Avg_eq) / 2;
            while (mm < 0 || de > 1)
            {
                await QsfpViewModel.WriteEqualizer(eq, ch);
                QsfpViewModel._model.Bertwave.DisplayAutoScale();
                await Task.Delay(100);

                double mmn = GetMM(ch);
                _ = MyMessage($"MaskMargin {mmn}");
                if (mm <= mmn)
                    eq += de;
                else eq -= de;

                de /= 2;
                mm = mmn;
            }
            if (mm < 0)
            {
                await QsfpViewModel.WriteEqualizer(old_eq, ch);
                QsfpViewModel._model.Bertwave.DisplayAutoScale();
                _ = MyMessage("Не удалось настроить MM");
            }
            else
                _ = MyMessage("MM настроен");
        }

        public async Task MyMessage(string message)
        {
            await Task.Run(() =>
            {
                _dispatcher.Invoke(new Action(() => Log.Add(message)));
                SbText = message;
            });
        }

        public async Task NewTune(Channel ch)
        {
            QsfpViewModel._model.Utb.IsAutoTune = true;
            _ = MyMessage($"Началась настройка {(int)ch+1}-го канала...");
            data.Clear();
            await Task.Run(() =>
            {
                double[] x = new double[2] { 270.0, 670.0 };
                
                nelMead(ch, ref x, 2, 5.0, 0.5, 1.0, 1.0, 0.5, 0.5);
                //var m = data.First(e => e.Value == data.Max(d => d.Value));
                //_ = MyMessage($"Найден максимум в точке ({m.Key.b}, {m.Key.m}) равный {Math.Round(m.Value, 2)}");
                //_ = f(ch, new MyTuple { x = m.Key.b, y = m.Key.m });
                _ = MyMessage($"Er={GetEr(ch)}, Snr={GetSnr(ch)}");
            });
            await TuneMM(ch);
            _ = MyMessage($"{(int)ch+1}-й канал Фсё!!!");

            QsfpViewModel._model.Utb.IsAutoTune = false;
            foreach (var elem in data)
            {
                QsfpViewModel.logger.Trace($"({elem.Key.b}, {elem.Key.m}) {elem.Value}");
            }
        }
        //От Романа
        #region АВТОНАСТРОЙКА ОТ РОМЫ

        private int delayTime = 500;
        public OperationResult cancelResult = new OperationResult(false, "Автонастройка отменена");

        #endregion


        #region Математика от Игоря

        private double f(Channel ch, MyTuple point)//x - Bias Current, y - Modulation
        { 
            List<Task> tasks = new List<Task>();
            QsfpViewModel._model.Utb.IsAutoTune = true;

            tasks.Add(Task.Factory.StartNew(async () => await QsfpViewModel.WriteBias(point.x, ch)));
            tasks.Add(Task.Factory.StartNew(async () => await QsfpViewModel.WriteModulation(point.y, ch)));
            Task.WaitAll(tasks.ToArray());

            QsfpViewModel._model.Bertwave.DisplayAutoScale();
            Task.Delay(delayTime).Wait();
            QsfpViewModel._model.Utb.IsAutoTune = false;
            double h_er = 4.36;
            double h_snr = 5.5;
            double koef = 100;
            double res_er = GetEr(ch);
            if (res_er < h_er) res_er -= (h_er - res_er) * koef;
            double res_snr = GetSnr(ch);
            if(res_snr < h_snr) res_snr -= (h_snr - res_snr) * koef;
            double result = res_er + res_snr;
            if(!data.ContainsKey((point.x, point.y)))
                Task.Run(()=>data.Add((point.x, point.y), result));
            return -result;
        }


        const int NP = 2; // NP - число аргументов функции
        double[,] simplex = new double[NP, NP + 1]; // NP + 1 - число вершин симплекса
        double[] FN = new double[NP + 1];

        #region Для MaskMargin

        double[] simplex1 = new double[2];
        double[] FN1 = new double[2];

        #endregion

        private void makeSimplex(double[] X, double L, Channel ch, int NP = 2 )
        {
            double qn, q2, r1, r2;
            int i, j;
            qn = Math.Sqrt(1.0 + NP) - 1.0;
            q2 = L / Math.Sqrt(2.0) * (double)NP;
            r1 = q2 * (qn + (double)NP);
            r2 = q2 * qn;
            for (i = 0; i < NP; i++) simplex[i, 0] = X[i];
            for (i = 1; i < NP + 1; i++)
                for (j = 0; j < NP; j++)
                    simplex[j, i] = X[j] + r2;
            for (i = 1; i < NP + 1; i++) simplex[i - 1, i] = simplex[i - 1, i] - r2 + r1;
            for (i = 0; i < NP + 1; i++)
            {
                for (j = 0; j < NP; j++) X[j] = simplex[j, i];
                FN[i] = f(ch, new MyTuple { x = (int)X[0], y = (int)X[1] });
                _ = MyMessage($"Начальная вершина {i} : ({(int)X[0]}, {(int)X[1]}) {FN[i]}");
            }
        }
        private double[] center_of_gravity(int k, int NP = 2) // Центр тяжести симплекса
        {
            int i, j;
            double s;
            double[] xc = new double[NP];
            for (i = 0; i < NP; i++)
            {
                s = 0;
                for (j = 0; j < NP + 1; j++) s += simplex[i, j];
                xc[i] = s;
            }
            for (i = 0; i < NP; i++) xc[i] = (xc[i] - simplex[i, k]) / (double)NP;
            return xc;
        }
        private void reflection(int k, double cR, int NP = 2) // Отражение вершины с номером k относительно центра тяжести
        {
            double[] xc = center_of_gravity(k, NP); // cR – коэффициент отражения
            for (int i = 0; i < NP; i++) simplex[i, k] = (1.0 + cR) * xc[i] - simplex[i, k];
        }
        private void reduction(int k, double gamma, int NP = 2) // Редукция симплекса к вершине k
        {
            int i, j; // gamma – коэффициент редукции
            double[] xk = new double[NP];
            for (i = 0; i < NP; i++) xk[i] = simplex[i, k];
            for (j = 0; j < NP; j++)
                for (i = 0; i < NP; i++)
                    simplex[i, j] = xk[i] + gamma * (simplex[i, j] - xk[i]);
            for (i = 0; i < NP; i++) simplex[i, k] = xk[i]; // Восстанавливаем симплекс в вершине k
        }
        private void shrinking_expansion(int k, double alpha_beta, int NP = 2) // Сжатие/растяжение симплекса. alpha_beta – коэффициент растяжения/сжатия
        {
            double[] xc = center_of_gravity(k, NP);
            for (int i = 0; i < NP; i++)
                simplex[i, k] = xc[i] + alpha_beta * (simplex[i, k] - xc[i]);
        }
        private double findL(double[] X2, int NP) // Длиина ребра симплекса
        {
            double L = 0;
            for (int i = 0; i < NP; i++) L += X2[i] * X2[i];
            return Math.Sqrt(L);
        }
        private double minval(double[] F, int N1, ref int imi) // Минимальный элемент массива и его индекс
        {
            double fmi = double.MaxValue, f;
            for (int i = 0; i < N1; i++)
            {
                f = F[i];
                if (f < fmi)
                {
                    fmi = f;
                    imi = i;
                }
            }
            return fmi;
        }
        private double maxval(double[] F, int N1, ref int ima) // Максимальный элемент массива и его индекс
        {
            double fma = double.MinValue, f;
            for (int i = 0; i < N1; i++)
            {
                f = F[i];
                if (f > fma)
                {
                    fma = f;
                    ima = i;
                }
            }

            return fma;
        }
        private void simplexRestore(Channel ch,int NP = 2) // Восстанавление симплекса
        {
            int i, imi = 0, imi2 = /*-1*/0;
            double fmi, fmi2 = double.MaxValue, f;
            double[] X = new double[NP], X2 = new double[NP];
            fmi = minval(FN, NP + 1, ref imi);
            for (i = 0; i < NP + 1; i++)
            {
                f = FN[i];
                if (f != fmi && f < fmi2)
                {
                    fmi2 = f;
                    imi2 = i;
                }
            }
            for (i = 0; i < NP; i++)
            {
                X[i] = simplex[i, imi];
                X2[i] = simplex[i, imi] - simplex[i, imi2];
            }
            makeSimplex(X, findL(X2, NP),ch, NP);
        }
        private bool notStopYet(double L_thres, int NP = 2) // Возвращает true, если длина хотя бы одного ребра симплекса превышает L_thres, или false - в противном случае
        {
            int i, j, k;
            double[] X = new double[NP], X2 = new double[NP];
            for (i = 0; i < NP; i++)
            {
                for (j = 0; j < NP; j++) X[j] = simplex[j, i];
                for (j = i + 1; j < NP + 1; j++)
                {
                    for (k = 0; k < NP; k++) X2[k] = X[k] - simplex[k, j];
                    if (findL(X2, NP) > L_thres) return true;
                }
            }
            return false;
        }

        private void nelMead(Channel ch,ref double[] X, int NP, double L, double L_thres, double cR, double alpha, double beta, double gamma)
        {
            int i, j2, imi = -1, ima = -1;
            int j = 0, kr = 0, jMx = MaxHops*2; // Предельное число шагов алгоритма (убрать после отладки)
            double[] X2 = new double[NP], X_R = new double[NP];
            double Fmi, Fma, F_R = 0, F_S, F_E;
            int kr_todo = MaxHops; // kr_todo - число шагов алгоритма, после выполнения которых симплекс восстанавливается 13
            _ = MyMessage($"Число шагов до восстановления {kr_todo}");
            makeSimplex(X, L, ch, NP);
            while (notStopYet(L_thres, NP) && j < jMx)
            {

                j++; // Число итераций
                kr++;
                if (kr == kr_todo)
                {
                    QsfpViewModel.logger.Trace("Восстановление симплекса");
                    kr = 0;
                    simplexRestore(ch,NP); // Восстановление симплекса
                }
                Fmi = minval(FN, NP + 1, ref imi);
                Fma = maxval(FN, NP + 1, ref ima); // ima - Номер отражаемой вершины
                for (i = 0; i < NP; i++)
                {
                    X[i] = simplex[i, ima];
                    if (X[0] < 200) X[0] = simplex[0, ima] = 300;
                    if (X[0] > 400) X[0] = simplex[0, ima] = 300;
                    if (X[1] < 550) X[1] = simplex[1, ima] = 700;
                    if (X[1] > 850) X[1] = simplex[1, ima] = 700;
                }
                var point = new MyTuple { x = (int)X[0], y = (int)X[1] };
                var er = GetEr(ch);
                var snr = GetSnr(ch);
                _ = MyMessage($"{j}. {Math.Round(X[0])}, {Math.Round(X[1])} : {Math.Round(f(ch, point),2)} Er={er}, Snr={snr}");
                reflection(ima, cR, NP); // Отражение 
                for (i = 0; i < NP; i++)
                {
                    X_R[i] = simplex[i, ima];
                    if (X_R[0] < 200) X_R[0] = simplex[0, ima] = 300;
                    if (X_R[0] > 400) X_R[0] = simplex[0, ima] = 300;
                    if (X_R[1] < 550) X_R[1] = simplex[1, ima] = 700;
                    if (X_R[1] > 850) X_R[1] = simplex[1, ima] = 700;

                }

                // Значение функции в вершине ima симплекса после отражения
                F_R = f(ch,new MyTuple { x = (int)X_R[0], y = (int)X_R[1] });

                if (F_R > Fma)
                {
                    shrinking_expansion(ima, beta, NP); // Сжатие
                    for (i = 0; i < NP; i++)
                    {
                        X2[i] = simplex[i, ima];
                        if (X2[0] < 200) X2[0] = simplex[0, ima] = 300;
                        if (X2[0] > 400) X2[0] = simplex[0, ima] = 300;
                        if (X2[1] < 550) X2[1] = simplex[1, ima] = 700;
                        if (X2[1] > 850) X2[1] = simplex[1, ima] = 700;

                    }

                    // Значение функции в вершине ima симплекса после его сжатия
                    F_S = f(ch, new MyTuple { x = (int)X2[0], y = (int)X2[1] });
                    if (F_S > Fma)
                    {
                        for (i = 0; i < NP; i++) simplex[i, ima] = X[i];//ima
                        reduction(ima, gamma, NP); // Редукция 
                        for (i = 0; i < NP + 1; i++)
                        {
                            if (i == ima) continue;
                            for (j2 = 0; j2 < NP; j2++) X2[j2] = simplex[j2, i];
                            // Значения функций в вершинах симплекса после редукции. В вершине ima значение функции сохраняется
                            FN[i] = f(ch, new MyTuple { x = (int)X2[0], y = (int)X2[1] });
                        }
                    }
                    else
                        FN[ima] = F_S;
                }
                else if (F_R < Fmi)
                {
                    shrinking_expansion(ima, alpha, NP); // Растяжение 
                    for (j2 = 0; j2 < NP; j2++) X2[j2] = simplex[j2, ima];
                    // Значение функции в вершине ima симплекса после его растяжения
                    F_E = f(ch, new MyTuple { x = (int)X2[0], y = (int)X2[1] });
                    if (F_E > Fmi)
                    {
                        for (j2 = 0; j2 < NP; j2++) simplex[j2, ima] = X_R[j2];
                        FN[ima] = F_R;
                    }
                    else
                        FN[ima] = F_E;
                }
                else
                    FN[ima] = F_R;
            }
        }

        #endregion
    }
}

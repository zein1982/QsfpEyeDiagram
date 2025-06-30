using Microsoft.EntityFrameworkCore;
using Std.Data.Database.Contexts;
using Std.Data.Database.Domain;

namespace QsfpEyeDiagram.Database
{
    public class QsfpEyeDiagramDataContextInstance : DataContext
    {
        public QsfpEyeDiagramDataContextInstance(string server, string user, string password, string database) : base(server, user, password, database)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Параметры записи о работнике
            modelBuilder.Entity<WorkerRecord>().Property(g => g.Code).HasColumnName("code").HasMaxLength(45).IsRequired();
            modelBuilder.Entity<WorkerRecord>().Property(g => g.FullName).HasColumnName("fullname").HasMaxLength(45).IsRequired();
            modelBuilder.Entity<WorkerRecord>().Property(g => g.Login).HasColumnName("login").HasMaxLength(45).IsRequired();
            modelBuilder.Entity<WorkerRecord>().Property(g => g.Password).HasColumnName("password").HasMaxLength(45).IsRequired();
            modelBuilder.Entity<WorkerRecord>().Property(g => g.Description).HasColumnName("description");

            modelBuilder.Entity<WorkerRecord>().ToTable("workers");
            #endregion

            //#region Параметры записи о калибровке глазковой диаграммы
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.UniqueId).HasColumnName("uniqueid").HasMaxLength(8).IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.WorkerCode).HasColumnName("workercode").HasMaxLength(25).IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.DateTime).HasColumnName("date").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.BertwaveIP).HasColumnName("bertwaveip").HasMaxLength(15).IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.OsaIP).HasColumnName("osaip").HasMaxLength(15).IsRequired();

            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.Crossing).HasColumnName("crossing").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.Snr).HasColumnName("snr").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.AveragePower).HasColumnName("averagepower").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.ExtinctionRatio).HasColumnName("extinctionratio").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.JitterPP).HasColumnName("jitterpp").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.JitterRms).HasColumnName("jitterrms").IsRequired();

            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.AverageWavelength).HasColumnName("averagewavelength").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.MinWavelength).HasColumnName("minwavelength").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.MaxWavelength).HasColumnName("maxwavelength").IsRequired();

            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.ModuleBias).HasColumnName("modulebias").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.ModuleModulation).HasColumnName("modulemodulation").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.ModuleNegativeBias).HasColumnName("modulenegativebias").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.ModuleCrossing).HasColumnName("modulecrossing").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.ModuleJitter).HasColumnName("modulejitter").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.ModulePreEmphasis).HasColumnName("modulepreemphasis").IsRequired();

            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.Current).HasColumnName("current").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.ApdCurrent).HasColumnName("apdcurrent").IsRequired();
            //modelBuilder.Entity<DwdmEyeRecord>().Property(g => g.ApdVoltage).HasColumnName("apdvoltage").IsRequired();

            //modelBuilder.Entity<DwdmEyeRecord>().ToTable("dwdmeye");
            //#endregion
        }
    }
}

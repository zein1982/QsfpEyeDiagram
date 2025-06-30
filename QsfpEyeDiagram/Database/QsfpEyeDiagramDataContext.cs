namespace QsfpEyeDiagram.Database
{
    public class QsfpEyeDiagramDataContext
    {
        private static QsfpEyeDiagramDataContextInstance _instance;
        public static QsfpEyeDiagramDataContextInstance Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new QsfpEyeDiagramDataContextInstance(
                        server: Properties.Settings.Default.DatabaseServer,
                        user: Properties.Settings.Default.DatabaseUser,
                        password: Properties.Settings.Default.DatabasePassword,
                        database: "production");//Properties.Settings.Default.DatabaseName);
                }

                return _instance;
            }
        }

        private QsfpEyeDiagramDataContext()
        {
        }
    }
}

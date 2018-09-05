namespace Words
{
    using System;
    using LiteDB;

    public class DatabaseWrapper : IDisposable
    {
        private readonly LiteDatabase database;

        public DatabaseWrapper(string filename, Action<string> loggerAction)
        {
            var logger = new Logger(
                Logger.FULL ^ Logger.DISK ^ Logger.LOCK ^ Logger.CACHE,
                loggerAction);
            database = new LiteDatabase(
                $"Filename={filename};Cache Size=0",
                log: logger);
        }

        static DatabaseWrapper()
        {
            var mapper = BsonMapper.Global;
            mapper.Entity<NormalizedToOriginal>()
                .Id(x => x.Normalized);
            mapper.Entity<WordPermutations>()
                .Id(x => x.NormalizedSorted);
        }

        public LiteCollection<NormalizedToOriginal> NormalizedToOriginals => database.GetCollection<NormalizedToOriginal>("normalized");

        public LiteCollection<WordPermutations> WordPermutations => database.GetCollection<WordPermutations>("permutations");

        public AdvancedWrapper Advanced => new AdvancedWrapper(database);

        public void Dispose()
        {
            database.Dispose();
        }

        public class AdvancedWrapper
        {
            public AdvancedWrapper(LiteDatabase database)
            {
                Database = database;
            }

            public LiteDatabase Database { get; }
        }
    }
}
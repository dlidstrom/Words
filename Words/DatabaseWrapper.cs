﻿namespace Words
{
    using System;
    using LiteDB;

    public class DatabaseWrapper : IDisposable
    {
        private readonly LiteDatabase database;

        public DatabaseWrapper(string filename)
        {
            database = new LiteDatabase($"filename={filename};upgrade=true");
        }

        static DatabaseWrapper()
        {
            var mapper = BsonMapper.Global;
            mapper.Entity<NormalizedToOriginal>()
                .Id(x => x.Normalized);
            mapper.Entity<WordPermutations>()
                .Id(x => x.NormalizedSorted);
        }

        public ILiteCollection<NormalizedToOriginal> NormalizedToOriginals => database.GetCollection<NormalizedToOriginal>("normalized");

        public ILiteCollection<WordPermutations> WordPermutations => database.GetCollection<WordPermutations>("permutations");

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
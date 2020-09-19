﻿using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shrimpbot.Services.Database
{
    public class DatabaseManager
    {
        public static string DatabasePath;
        static DatabaseManager()
        {
            DatabasePath = Path.Combine(Directory.GetCurrentDirectory(),
                                                "Database"
                                                );
        }
        public static DatabaseUser GetUser(LiteDatabase db, ulong id)
        {
            DatabaseUser user;

            user = db.GetCollection<DatabaseUser>("Users").FindOne(x => id == x.Id);
            if (user is null)
            {
                Console.WriteLine($"Making a new user!");
                db.GetCollection<DatabaseUser>("Users").Insert(new DatabaseUser { Id = id });
                user = db.GetCollection<DatabaseUser>("Users").FindOne(x => id == x.Id);
            }
            return user;
        }
        public static void WriteUser(LiteDatabase db, DatabaseUser user)
        {
            if (!db.GetCollection<DatabaseUser>("Users").Update(user))
            {
                db.GetCollection<DatabaseUser>("Users").Insert(user);
            }
        }
    }
}

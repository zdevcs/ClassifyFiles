﻿using ClassifyFiles.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static ClassifyFiles.Util.DbUtility;

namespace ClassifyFiles.Util
{
    public static class ClassUtility
    {

        public static async Task<List<Class>> GetClassesAsync(Project project)
        {
            Debug.WriteLine("db: " + nameof(GetClassesAsync));

            List<Class> classes = await db.Classes
                .Where(p => p.Project == project)
                .Include(p => p.MatchConditions)
                .ToListAsync();

            return classes;
        }

        public static async Task<Class> AddClassAsync(Project project)
        {
            Debug.WriteLine("db: " + nameof(AddClassAsync));

            Class c = new Class() { Project = project, Name = "未命名类" };
            db.Classes.Add(c);
            await db.SaveChangesAsync();
            return c;
        }
        public static async Task DeleteClassAsync(Class c)
        {
            Debug.WriteLine("db: " + nameof(DeleteClassAsync));
            db.Entry(c).State = EntityState.Deleted;
            await db.SaveChangesAsync();
        }

        public static async Task SaveClassAsync(Class c)
        {
            Debug.WriteLine("db: " + nameof(SaveClassAsync));

            if (await db.Classes.AnyAsync(p => p.ID == c.ID))
            {
                db.Entry(c).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
        }

        public static Task<int> GetClassesCountAsync(Project project)
        {
            return db.Classes.CountAsync(p => p.Project == project);
        }
    }
}
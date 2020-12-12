using System.Collections.Generic;
using System.Threading.Tasks;
using DataServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DataServer.Persistence.Impl
{
    public class UserReportImpl : IUserReport_Persistence
    {
        MapDbContext dbContext;

        public UserReportImpl()
        {
            dbContext = new MapDbContext();
        }
        public async Task CreateUserReport(Report<User> userReport)
        {
            EntityEntry<Report<User>> newlyAdded = await dbContext.UserReports.AddAsync(userReport);
            await dbContext.SaveChangesAsync();
        }

        public async Task DismissUserReport(long reportId)
        {
            Report<User> toDismiss = await dbContext.PlaceReports.FirstOrDefaultAsync(ur => ur.reportId == reportId);
            toDismiss.resolved = true;
        }

        public async Task<Dictionary<long, Report<User>>> GetUserReports()
        {
            List<Report<User>> myList = await dbContext.UserReports.ToListAsync();
            Dictionary<long, Report<User>> myDic = new Dictionary<long, Report<User>>();
            foreach (Report<User> item in myList)
            {
                myDic.Add(item.reportId, item);
            }
            return myDic;
        }

        public async Task UpdateUserReport(Report<User> userReport)
        {
            dbContext.UserReports.Update(userReport);
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.ConstraintLibrary;

namespace SimpleSudoku.SudokuSolver.Services
{
    public static class ServiceExtension
    {
        public static void AddConstraintSolver(this IServiceCollection services)
        {
            services.AddSingleton<IConstraintManager, ConstraintManager>();
            services.AddSingleton<IConstraintSolver, ConstraintSolver>(p => new ConstraintSolver(
                    p.GetRequiredService<IConstraintManager>(),
                    p.GetRequiredService<IPuzzleModel>()
                ));
        }
    }
}

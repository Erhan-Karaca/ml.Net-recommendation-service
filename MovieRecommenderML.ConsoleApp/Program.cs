// This file was auto-generated by ML.NET Model Builder. 

using System;
using System.Linq;
using MovieRecommenderML.Model;

namespace MovieRecommenderML.ConsoleApp
{
    
    class Program
    {
        static void Main(string[] args)
        {
            //ModelBuilder.CreateModel();

            // Create single instance of sample data from first line of dataset for model input
            ModelInput sampleData = null;

            if (1 == 2)
            {
                sampleData = new ModelInput()
                {
                    UserId = 1F,
                    MovieId = 1097F,
                };
            }
            else
            {
                sampleData = new ModelInput()
                {
                    UserId = 2F,
                    MovieId = 220F,
                };
            }

            Mysql mysql = new Mysql();
            var top5 = (from m in mysql.GetAllMovie(Convert.ToInt32(sampleData.UserId))
                        let p = ConsumeModel.Predict(
                           new ModelInput()
                           {
                               UserId = 2,
                               MovieId = m.MovieId
                           }
                        )
                        orderby p.Score descending
                        select (MovieId: m.MovieId, Score: p.Score)
                        ).Take(20);

            foreach (var t in top5)
            Console.WriteLine($"  Score:{t.Score}\tMovie: {mysql.GetMovie(Convert.ToInt32(t.MovieId))?.MovieName}");

            // Make a single prediction on the sample data and print results
            var predictionResult = ConsumeModel.Predict(sampleData);

            Console.WriteLine("Using model to make single prediction -- Comparing actual Rating with predicted Rating from sample data...\n\n");
            Console.WriteLine($"UserId: {sampleData.UserId}");
            Console.WriteLine($"MovieId: {sampleData.MovieId}");
            Console.WriteLine($"\n\nPredicted Rating: {predictionResult.Score}\n\n");
            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }
    }
}

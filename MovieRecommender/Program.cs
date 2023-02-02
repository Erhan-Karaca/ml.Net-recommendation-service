// <SnippetUsingStatements>
using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using MovieRecommender.DataStructures;

// </SnippetUsingStatements>

namespace MovieRecommender
{

    class Program
    {
        // Using the ml-latest-small.zip as dataset from https://grouplens.org/datasets/movielens/. 
        private static string ModelsRelativePath = @"../../../MLModels";
        public static string DatasetsRelativePath = @"../../../Data";

        private static string TrainingDataRelativePath = $"{DatasetsRelativePath}/recommendation-ratings-train.csv";
        private static string TestDataRelativePath = $"{DatasetsRelativePath}/recommendation-ratings-test.csv";
        private static string MoviesDataLocation = $"{DatasetsRelativePath}/movies.csv";

        private static string TrainingDataLocation = GetAbsolutePath(TrainingDataRelativePath);
        private static string TestDataLocation = GetAbsolutePath(TestDataRelativePath);

        private static string ModelPath = GetAbsolutePath(ModelsRelativePath);

        private const float predictionuserId = 6;
        private const int predictionmovieId = 10;

        static void Main(string[] args)
        {
            //STEP 1: Create MLContext to be shared across the model creation workflow objects 
            MLContext mlcontext = new MLContext();

            //STEP 2: Read the training data which will be used to train the movie recommendation model    
            //The schema for training data is defined by type 'TInput' in LoadFromTextFile<TInput>() method.
            IDataView trainingDataView = mlcontext.Data.LoadFromTextFile<MovieRating>(TrainingDataLocation, hasHeader: true, separatorChar: ',');

            //STEP 3: Transform your data by encoding the two features userId and movieID. These encoded features will be provided as input
            //        to our MatrixFactorizationTrainer.
            var dataProcessingPipeline = mlcontext.Transforms.Conversion.MapValueToKey(outputColumnName: "userIdEncoded", inputColumnName: nameof(MovieRating.userId))
                           .Append(mlcontext.Transforms.Conversion.MapValueToKey(outputColumnName: "movieIdEncoded", inputColumnName: nameof(MovieRating.movieId)));

            //Specify the options for MatrixFactorization trainer            
            MatrixFactorizationTrainer.Options options = new MatrixFactorizationTrainer.Options();
            options.MatrixColumnIndexColumnName = "userIdEncoded";
            options.MatrixRowIndexColumnName = "movieIdEncoded";
            options.LabelColumnName = "Label";
            options.NumberOfIterations = 20;
            options.ApproximationRank = 100;

            //STEP 4: Create the training pipeline 
            var trainingPipeLine = dataProcessingPipeline.Append(mlcontext.Recommendation().Trainers.MatrixFactorization(options));

            //STEP 5: Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============");
            ITransformer model = trainingPipeLine.Fit(trainingDataView);

            //STEP 6: Evaluate the model performance 
            Console.WriteLine("=============== Evaluating the model ===============");
            IDataView testDataView = mlcontext.Data.LoadFromTextFile<MovieRating>(TestDataLocation, hasHeader: true, separatorChar: ',');
            var prediction = model.Transform(testDataView);
            var metrics = mlcontext.Regression.Evaluate(prediction, labelColumnName: "Label", scoreColumnName: "Score");
            Console.WriteLine("The model evaluation metrics RootMeanSquaredError:" + metrics.RootMeanSquaredError);

            //STEP 7:  Try/test a single prediction by predicting a single movie rating for a specific user
            var predictionengine = mlcontext.Model.CreatePredictionEngine<MovieRating, MovieRatingPrediction>(model);
            /* Make a single movie rating prediction, the scores are for a particular user and will range from 1 - 5. 
               The higher the score the higher the likelyhood of a user liking a particular movie.
               You can recommend a movie to a user if say rating > 3.5.*/
            var movieratingprediction = predictionengine.Predict(
                new MovieRating()
                {
                    //Example rating prediction for userId = 6, movieId = 10 (GoldenEye)
                    userId = predictionuserId,
                    movieId = predictionmovieId
                }
            );

            Movie movieService = new Movie();
            Console.WriteLine("For userId:" + predictionuserId + " movie rating prediction (1 - 5 stars) for movie:" + movieService.Get(predictionmovieId).movieTitle + " is:" + Math.Round(movieratingprediction.Score, 1));

            // find the top 5 movies for a given user
            Console.WriteLine("Calculating the top 5 movies for user 6...");
            var top5 = (from m in movieService.All()
                        let p = predictionengine.Predict(
                           new MovieRating()
                           {
                               userId = 6,
                               movieId = m.movieId
                           }
                        )
                        orderby p.Score descending
                        select (MovieId: m.movieId, Score: p.Score)
                        ).Take(10);

            foreach (var t in top5)
                Console.WriteLine($"  Score:{t.Score}\tMovie: {movieService.Get(t.MovieId)?.movieTitle}");

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadLine();
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }

    /*
    class Program
    {
        static void Main(string[] args)
        {

            // Create MLContext to be shared across the model creation workflow objects
            // <SnippetMLContext>
            MLContext mlContext = new MLContext();
            // </SnippetMLContext>

            // Load data
            // <SnippetLoadDataMain>
            (IDataView trainingDataView, IDataView testDataView) = LoadData(mlContext);
            // </SnippetLoadDataMain>

            // Build & train model
            // <SnippetBuildTrainModelMain>
            ITransformer model = BuildAndTrainModel(mlContext, trainingDataView);
            // </SnippetBuildTrainModelMain>

            // Evaluate quality of model
            // <SnippetEvaluateModelMain>
            EvaluateModel(mlContext, testDataView, model);
            // </SnippetEvaluateModelMain>

            // Use model to try a single prediction (one row of data)
            // <SnippetUseModelMain>
            UseModelForSinglePrediction(mlContext, model, 1, 1);
            UseModelForSinglePrediction(mlContext, model, 1, 2);
            UseModelForSinglePrediction(mlContext, model, 1, 3);
            UseModelForSinglePrediction(mlContext, model, 1, 4);
            UseModelForSinglePrediction(mlContext, model, 1, 5);
            UseModelForSinglePrediction(mlContext, model, 1, 6);
            UseModelForSinglePrediction(mlContext, model, 1, 7);
            UseModelForSinglePrediction(mlContext, model, 1, 8);
            UseModelForSinglePrediction(mlContext, model, 1, 9);
            UseModelForSinglePrediction(mlContext, model, 2, 1);
            UseModelForSinglePrediction(mlContext, model, 2, 2);
            UseModelForSinglePrediction(mlContext, model, 2, 3);
            UseModelForSinglePrediction(mlContext, model, 2, 4);
            UseModelForSinglePrediction(mlContext, model, 2, 5);
            UseModelForSinglePrediction(mlContext, model, 2, 6);
            UseModelForSinglePrediction(mlContext, model, 2, 7);
            UseModelForSinglePrediction(mlContext, model, 2, 8);
            UseModelForSinglePrediction(mlContext, model, 2, 9);
            // </SnippetUseModelMain>

            // Save model
            // <SnippetSaveModelMain>
            SaveModel(mlContext, trainingDataView.Schema, model);
            // </SnippetSaveModelMain>
        }

        // Load data
        public static (IDataView training, IDataView test) LoadData(MLContext mlContext)
        {
            // Load training & test datasets using datapaths
            // <SnippetLoadData>
            var trainingDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "recommendation-ratings-train.csv");
            var testDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "recommendation-ratings-test.csv");
            
            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<MovieRating>(trainingDataPath, hasHeader: true, separatorChar: ',');
            IDataView testDataView = mlContext.Data.LoadFromTextFile<MovieRating>(testDataPath, hasHeader: true, separatorChar: ',');

            return (trainingDataView, testDataView);
            // </SnippetLoadData>
        }

        // Build and train model
        public static ITransformer BuildAndTrainModel(MLContext mlContext, IDataView trainingDataView)
        {
            // Add data transformations
            // <SnippetDataTransformations>
            IEstimator<ITransformer> estimator = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "userIdEncoded", inputColumnName: "userId")
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "movieIdEncoded", inputColumnName: "movieId"));
            // </SnippetDataTransformations>

            // Set algorithm options and append algorithm
            // <SnippetAddAlgorithm>
            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "userIdEncoded",
                MatrixRowIndexColumnName = "movieIdEncoded",
                LabelColumnName = "Label",
                NumberOfIterations = 20,
                ApproximationRank = 100
            };

            var trainerEstimator = estimator.Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));
            // </SnippetAddAlgorithm>

            // <SnippetFitModel>
            Console.WriteLine("=============== Training the model ===============");
            ITransformer model = trainerEstimator.Fit(trainingDataView);

            return model;
            // </SnippetFitModel>
        }

        // Evaluate model
        public static void EvaluateModel(MLContext mlContext, IDataView testDataView, ITransformer model)
        {
            // Evaluate model on test data & print evaluation metrics
            // <SnippetTransform>
            Console.WriteLine("=============== Evaluating the model ===============");
            var prediction = model.Transform(testDataView);
            // </SnippetTransform>

            // <SnippetEvaluate>
            var metrics = mlContext.Regression.Evaluate(prediction, labelColumnName: "Label", scoreColumnName: "Score");
            // </SnippetEvaluate>

            // <SnippetPrintMetrics>
            Console.WriteLine("Root Mean Squared Error : " + metrics.RootMeanSquaredError.ToString());
            Console.WriteLine("RSquared: " + metrics.RSquared.ToString());
            // </SnippetPrintMetrics>
        }

        // Use model for single prediction
        public static void UseModelForSinglePrediction(MLContext mlContext, ITransformer model, float pUserId, float pMovieId)
        {
            // <SnippetPredictionEngine>
            Console.WriteLine("=============== Making a prediction ===============");
            var predictionEngine = mlContext.Model.CreatePredictionEngine<MovieRating, MovieRatingPrediction>(model);
            // </SnippetPredictionEngine>

            // Create test input & make single prediction
            // <SnippetMakeSinglePrediction>
            var testInput = new MovieRating { userId = pUserId, movieId = pMovieId };

            var movieRatingPrediction = predictionEngine.Predict(testInput);
            // </SnippetMakeSinglePrediction>

            // <SnippetPrintResults>
            if (Math.Round(movieRatingPrediction.Score, 1) > 3.5)
            {
                Console.WriteLine("Movie " + testInput.movieId + " is recommended for user " + testInput.userId);
            }
            else
            {
                Console.WriteLine("Movie " + testInput.movieId + " is not recommended for user " + testInput.userId);
            }
            // </SnippetPrintResults>
        }

        //Save model
        public static void SaveModel(MLContext mlContext, DataViewSchema trainingDataViewSchema, ITransformer model)
        {
            // Save the trained model to .zip file
            // <SnippetSaveModel>
            var modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "MovieRecommenderModel.zip");

            Console.WriteLine("=============== Saving the model to a file ===============");
            mlContext.Model.Save(model, trainingDataViewSchema, modelPath);
            // </SnippetSaveModel>
        }
    }
    */
}
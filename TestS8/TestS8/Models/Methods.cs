namespace TestS8.Models
{
    public class Methods
    {
        public bool Analytique { get; set; }
        public bool KNN { get; set; }
        public bool RandomForest { get; set; }
        public bool SVM { get; set; }

        // Random Forest
        public double n_estimators { get; set; }
        public double max_depth { get; set; }
        public double min_samples_split { get; set; }
        public double min_samples_leaf { get; set; }
        public string ?bootstrap { get; set; }

        // KNN
        public double n_neighbors {  get; set; }
        public string ?weights { get; set; }
        public string ?algorithm { get; set; }
        public double p_knn { get; set; }

        //SVM
        public string? kernel {  get; set; }
        public double C {  get; set; }
        public string ?probability { get; set; }
        public double tol { get; set; }
    }


}

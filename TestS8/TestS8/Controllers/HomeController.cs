using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using TestS8.Models;

namespace TestS8.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        public IActionResult Index()
        {
            if (User.IsInRole("Lambda"))
            {
                return RedirectToAction("Guest", "Home");
            }
            else if (User.IsInRole("Admin"))
            {
                return RedirectToAction("IndexAdmin", "Home");
            }
            else
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            var model = new Methods
            {

            };
            return View(model);
        }

        public async Task<IActionResult> Upload(IFormFile file)  // Ceci est une méthode d'action; chaque méthode d'action correspond à un URL du site
        {
            // Chemin de destination pour enregistrer le fichier dans le sous-répertoire spécifié
            var filePath = Path.Combine(Directory.GetCurrentDirectory(),"Python", "data.zip");

            // Enregistrement du fichier sur le serveur
            using (var stream = new FileStream(filePath, FileMode.Create)) // Créer un flux de fichier pour enregistrer le fichier
            {
                await file.CopyToAsync(stream); // Copier les données du fichier téléchargé dans le flux de fichier de destination
            }
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync("http://localhost:5000/pretraitement", null);

            }
            return View("Index");
        }

        public async Task<IActionResult> Valider(Methods model)  // Ceci est une méthode d'action; chaque méthode d'action correspond à un URL du site
        {
            if (ModelState.IsValid)
            {
                if (model.Analytique)
                {
                    // Récupérer le lien du fichier uploadé et l'envoyer dans la requpete http au serveur python
                    // Préparation de la requête http à envoyer au service python (flask_api_test.py)
                    using (var client = new HttpClient())
                    {
                        var response = await client.PostAsync("http://localhost:5000/analytical", null);
                        var result = await response.Content.ReadAsStringAsync();

                        ViewBag.Result = result;
                    }
                }
                if (model.KNN)
                {
                    // Préparation de la requête http à envoyer au service python (flask_api_test.py)
                    using (var client = new HttpClient())
                    {
                        var requestData = new
                        {
                            Parameter1 = model.n_neighbors,
                            Parameter2 = model.weights,
                            Parameter3 = model.algorithm,
                            Parameter4 = model.p_knn
                        };

                        var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                        var response = await client.PostAsync("http://localhost:5000/knn", content);
                        var result = await response.Content.ReadAsStringAsync();
                        ViewBag.Result = result;
                    }
                }
                if (model.RandomForest)
                {
                    // Préparation de la requête http à envoyer au service python (flask_api_test.py)
                    using (var client = new HttpClient())
                    {
                        var requestData = new
                        {
                            Parameter1 = model.n_estimators,
                            Parameter2 = model.max_depth,
                            Parameter3 = model.min_samples_split,
                            Parameter4 = model.min_samples_leaf,
                            Parameter5 = model.bootstrap
                        };

                        var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                        var response = await client.PostAsync("http://localhost:5000/randomforest", content);
                        var result = await response.Content.ReadAsStringAsync();
                        ViewBag.Result = result;
                    }
                    
                }
                if (model.SVM)
                {
                    // Préparation de la requête http à envoyer au service python (flask_api_test.py)
                    using (var client = new HttpClient())
                    {
                        var requestData = new
                        {
                            Parameter1 = model.kernel,
                            Parameter2 = model.C,
                            Parameter3 = model.probability,
                            Parameter4 = model.tol
                        };

                        var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                        var response = await client.PostAsync("http://localhost:5000/svm", content);
                        var result = await response.Content.ReadAsStringAsync();
                        ViewBag.Result = result;
                    }
                }
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Python", "data.zip");
            var filePath2 = Path.Combine(Directory.GetCurrentDirectory(), "Python", "data_anonymous");
            try
            {
                // Suppression du fichier
                System.IO.File.Delete(filePath);
                //System.IO.File.Delete(filePath2);
                Console.WriteLine("Fichier supprimé avec succès.");
            }
            catch (IOException ioExp)
            {
                Console.WriteLine("Erreur lors de la suppression du fichier: " + ioExp.Message);
            }
            catch (UnauthorizedAccessException unAuthExp)
            {
                Console.WriteLine("Accès refusé: " + unAuthExp.Message);
            }
            // Rediriger vers une autre vue ou retourner un résultat
            //return RedirectToAction("SuccessPage");
            return View("Index");
        }


        public async Task<IActionResult> RandomForest(int Param1, int Param2, int Param3, int Param4, string Param5)  // Ceci est une méthode d'action; chaque méthode d'action correspond à un URL du site
        {
            // Préparation de la requête http à envoyer au service python (flask_api_test.py)
            using (var client = new HttpClient())
            {
                var requestData = new
                {
                    Parameter1 = Param1,
                    Parameter2 = Param2,
                    Parameter3 = Param3,
                    Parameter4 = Param4,
                    Parameter5 = Param5
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/randomForest", content);
                var result = await response.Content.ReadAsStringAsync();
                ViewBag.Result = result;
            }
            return View("Index");
        }

        public async Task<IActionResult> KNN(int Param1, string Param2, string Param3, int Param4)  // Ceci est une méthode d'action; chaque méthode d'action correspond à un URL du site
        {
            // Préparation de la requête http à envoyer au service python (flask_api_test.py)
            using (var client = new HttpClient())
            {
                var requestData = new
                {
                    Parameter1 = Param1,
                    Parameter2 = Param2,
                    Parameter3 = Param3,
                    Parameter4 = Param4
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/knn", content);
                var result = await response.Content.ReadAsStringAsync();
                ViewBag.Result = result;
            }
            return View("Index");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult IndexAdmin()
        {
            return View("Index");
        }
        
        [Authorize(Roles = "Lambda")]
        public IActionResult Guest()
        {
            return View();
        }

        [Authorize(Roles ="Admin")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

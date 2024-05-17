#Service Web Python indépendant du service C#
import string
from token import STRING
from tokenize import Double
from flask import Flask, request, jsonify
from analytique import analytical
from randomforest import random_forest
from knn import knn_model
from svm import svm_model
from analytiqueg import analyticalg
from randomforestg import random_forestg
from knng import knn_modelg
from svmg import svm_modelg
#from knn import knn_model
from pretraitement import pretraitement
import time
app = Flask(__name__)

@app.route('/pretraitement', methods=['POST'])
def pretraitement_route():
    pretraitement()


@app.route('/analytical', methods=['POST'])
def analytical_route():
    start = time.time()
    # Call the predict() function to make a prediction
    accuracy = analytical()  
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'time':time.time() - start})

@app.route('/randomforest', methods=['POST'])
def randomforest_route():
    start = time.time()
    # Get the input parameters from the request
    input_params = request.get_json()      
    if input_params is not None:
        n_estimators_rm = input_params.get('Parameter1')
        max_depth_rm = input_params.get('Parameter2')
        min_samples_split_rm = input_params.get('Parameter3')
        min_samples_leaf_rm = input_params.get('Parameter4')
        bootstrap_rm = input_params.get('Parameter5')
    # Call the predict() function to make a prediction
    accuracy = random_forest(int(n_estimators_rm), int(max_depth_rm),  int(min_samples_split_rm), int(min_samples_leaf_rm), bool(bootstrap_rm))    
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'time':time.time() - start})

@app.route('/knn', methods=['POST'])
def knn_route():
    start = time.time()
    # Get the input parameters from the request
    input_params = request.get_json()      
    if input_params is not None:
        n_neighbors_knn = input_params.get('Parameter1')
        weights_knn = input_params.get('Parameter2')
        algorithm = input_params.get('Parameter3')
        p_knn = input_params.get('Parameter4')
    # Call the predict() function to make a prediction
    accuracy = knn_model(int(n_neighbors_knn), str(weights_knn),  str(algorithm), float(p_knn))  
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'time':time.time() - start})

@app.route('/svm', methods=['POST'])
def svm_route():
    start = time.time()
    # Get the input parameters from the request
    input_params = request.get_json()      
    if input_params is not None:
        kernel_svm = input_params.get('Parameter1')
        C_svm = input_params.get('Parameter2')
        probability_svm = input_params.get('Parameter3')
        tol_svm = input_params.get('Parameter4')
    # Call the predict() function to make a prediction
    accuracy = svm_model(str(kernel_svm),float(C_svm),bool(probability_svm),float(tol_svm))  
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'time':time.time() - start})

#Guest
@app.route('/analyticalguest', methods=['POST'])
def analytical_route_guest():
    start = time.time()
    # Call the predict() function to make a prediction
    accuracy = analyticalg()  
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'time':time.time() - start})

@app.route('/randomforestguest', methods=['POST'])
def randomforest_route_guest():
    start = time.time()
    # Get the input parameters from the request
    input_params = request.get_json()      
    if input_params is not None:
        n_estimators_rm = input_params.get('Parameter1')
        max_depth_rm = input_params.get('Parameter2')
        min_samples_split_rm = input_params.get('Parameter3')
        min_samples_leaf_rm = input_params.get('Parameter4')
        bootstrap_rm = input_params.get('Parameter5')
    # Call the predict() function to make a prediction
    accuracy = random_forestg(int(n_estimators_rm), int(max_depth_rm),  int(min_samples_split_rm), int(min_samples_leaf_rm), bool(bootstrap_rm))    
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'time':time.time() - start})

@app.route('/knnguest', methods=['POST'])
def knn_route_guest():
    start = time.time()
    # Get the input parameters from the request
    input_params = request.get_json()      
    if input_params is not None:
        n_neighbors_knn = input_params.get('Parameter1')
        weights_knn = input_params.get('Parameter2')
        algorithm = input_params.get('Parameter3')
        p_knn = input_params.get('Parameter4')
    # Call the predict() function to make a prediction
    accuracy = knn_modelg(int(n_neighbors_knn), str(weights_knn),  str(algorithm), float(p_knn))  
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'time':time.time() - start})

@app.route('/svmguest', methods=['POST'])
def svm_route_guest():
    start = time.time()
    # Get the input parameters from the request
    input_params = request.get_json()      
    if input_params is not None:
        kernel_svm = input_params.get('Parameter1')
        C_svm = input_params.get('Parameter2')
        probability_svm = input_params.get('Parameter3')
        tol_svm = input_params.get('Parameter4')
    # Call the predict() function to make a prediction
    accuracy = svm_modelg(str(kernel_svm),float(C_svm),bool(probability_svm),float(tol_svm))  
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'time':time.time() - start})

app.run(host='0.0.0.0', port=5000)

using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    public class Graph
    {
        // Instance de la matrice d'adjacence
        private Matrix _matrix;

        // Indique si le graphe est orienté
        private bool _directed;

        // Valeur représentant l'absence d'arc
        private float _noEdgeValue;

        // Dictionnaire pour associer un nom de sommet à son index (ligne/colonne) dans la matrice
        private Dictionary<string, int> _vertexIndices;

        // Liste pour stocker les noms des sommets (permet de retrouver le nom à partir de l'index)
        private List<string> _vertexNames;

        // Liste pour stocker les valeurs associées aux sommets (synchronisée avec les index)
        private List<float> _vertexValues;


        // --- Construction du graphe ---

        // Contruit un graphe (`directed`=true => orienté)
        // La valeur `noEdgeValue` est le poids modélisant l'absence d'un arc (0 par défaut)
        public Graph(bool directed, float noEdgeValue = 0)
        {
            _directed = directed;
            _noEdgeValue = noEdgeValue;

            // On initialise une matrice vide 0x0
            _matrix = new Matrix(0, 0, noEdgeValue);

            _vertexIndices = new Dictionary<string, int>();
            _vertexNames = new List<string>();
            _vertexValues = new List<float>();
        }


        // --- Propriétés ---

        // Propriété : ordre du graphe
        // Lecture seule
        public int Order
        {
            get { return _vertexNames.Count; }
        }

        // Propriété : graphe orienté ou non
        // Lecture seule
        public bool Directed
        {
            get { return _directed; }
        }


        // --- Gestion des sommets ---

        // Ajoute le sommet de nom `name` et de valeur `value` (0 par défaut) dans le graphe
        // Lève une ArgumentException s'il existe déjà un sommet avec le même nom dans le graphe
        public void AddVertex(string name, float value = 0)
        {
            if (_vertexIndices.ContainsKey(name))
            {
                throw new ArgumentException($"Le sommet '{name}' existe déjà.");
            }

            int newIndex = Order; // Le nouvel index est égal à la taille actuelle (ajout à la fin)

            // Mise à jour des structures de données
            _vertexIndices[name] = newIndex;
            _vertexNames.Add(name);
            _vertexValues.Add(value);

            // Mise à jour de la matrice : ajout d'une ligne et d'une colonne à la fin
            _matrix.AddRow(newIndex);
            _matrix.AddColumn(newIndex);
        }


        // Supprime le sommet de nom `name` du graphe (et tous les arcs associés)
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public void RemoveVertex(string name)
        {
            if (!_vertexIndices.ContainsKey(name))
            {
                throw new ArgumentException($"Le sommet '{name}' n'existe pas.");
            }

            int index = _vertexIndices[name];

            // Suppression dans la matrice
            _matrix.RemoveRow(index);
            _matrix.RemoveColumn(index);

            // Suppression dans les listes
            _vertexNames.RemoveAt(index);
            _vertexValues.RemoveAt(index);

            // Reconstruction du dictionnaire d'indices
            // (car la suppression décale tous les indices suivants de -1)
            _vertexIndices.Clear();
            for (int i = 0; i < _vertexNames.Count; i++)
            {
                _vertexIndices[_vertexNames[i]] = i;
            }
        }

        // Renvoie la valeur du sommet de nom `name`
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public float GetVertexValue(string name)
        {
            if (!_vertexIndices.ContainsKey(name))
            {
                throw new ArgumentException($"Le sommet '{name}' n'existe pas.");
            }
            return _vertexValues[_vertexIndices[name]];
        }

        // Affecte la valeur du sommet de nom `name` à `value`
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public void SetVertexValue(string name, float value)
        {
            if (!_vertexIndices.ContainsKey(name))
            {
                throw new ArgumentException($"Le sommet '{name}' n'existe pas.");
            }
            _vertexValues[_vertexIndices[name]] = value;
        }


        // Renvoie la liste des noms des voisins du sommet de nom `vertexName`
        // (si ce sommet n'a pas de voisins, la liste sera vide)
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public List<string> GetNeighbors(string vertexName)
        {
            if (!_vertexIndices.ContainsKey(vertexName))
            {
                throw new ArgumentException($"Le sommet '{vertexName}' n'existe pas.");
            }

            List<string> neighbors = new List<string>();
            int index = _vertexIndices[vertexName];

            // On parcourt la ligne correspondant au sommet
            for (int j = 0; j < _matrix.NbColumns; j++)
            {
                // Si la valeur est différente de _noEdgeValue, c'est qu'il y a un arc
                if (_matrix.GetValue(index, j) != _noEdgeValue)
                {
                    neighbors.Add(_vertexNames[j]);
                }
            }

            return neighbors;
        }

        // --- Gestion des arcs ---

        /* Ajoute un arc allant du sommet nommé `sourceName` au sommet nommé `destinationName`, avec le poids `weight` (1 par défaut)
         * Si le graphe n'est pas orienté, ajoute aussi l'arc inverse, avec le même poids
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - il existe déjà un arc avec ces extrémités
         */
        public void AddEdge(string sourceName, string destinationName, float weight = 1)
        {
            if (!_vertexIndices.ContainsKey(sourceName) || !_vertexIndices.ContainsKey(destinationName))
            {
                throw new ArgumentException("Un des sommets n'existe pas.");
            }

            int i = _vertexIndices[sourceName];
            int j = _vertexIndices[destinationName];

            // Vérification si l'arc existe déjà
            if (_matrix.GetValue(i, j) != _noEdgeValue)
            {
                throw new ArgumentException("L'arc existe déjà.");
            }

            // Ajout de l'arc
            _matrix.SetValue(i, j, weight);

            // Si non orienté, on ajoute l'arc symétrique
            if (!_directed)
            {
                _matrix.SetValue(j, i, weight);
            }
        }

        /* Supprime l'arc allant du sommet nommé `sourceName` au sommet nommé `destinationName` du graphe
         * Si le graphe n'est pas orienté, supprime aussi l'arc inverse
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - l'arc n'existe pas
         */
        public void RemoveEdge(string sourceName, string destinationName)
        {
            if (!_vertexIndices.ContainsKey(sourceName) || !_vertexIndices.ContainsKey(destinationName))
            {
                throw new ArgumentException("Un des sommets n'existe pas.");
            }

            int i = _vertexIndices[sourceName];
            int j = _vertexIndices[destinationName];

            if (_matrix.GetValue(i, j) == _noEdgeValue)
            {
                throw new ArgumentException("L'arc n'existe pas.");
            }

            // Suppression de l'arc (on remet la valeur par défaut)
            _matrix.SetValue(i, j, _noEdgeValue);

            if (!_directed)
            {
                _matrix.SetValue(j, i, _noEdgeValue);
            }
        }

        /* Renvoie le poids de l'arc allant du sommet nommé `sourceName` au sommet nommé `destinationName`
         * Si le graphe n'est pas orienté, GetEdgeWeight(A, B) = GetEdgeWeight(B, A) 
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - l'arc n'existe pas
         */
        public float GetEdgeWeight(string sourceName, string destinationName)
        {
            if (!_vertexIndices.ContainsKey(sourceName) || !_vertexIndices.ContainsKey(destinationName))
            {
                throw new ArgumentException("Un des sommets n'existe pas.");
            }

            int i = _vertexIndices[sourceName];
            int j = _vertexIndices[destinationName];

            float value = _matrix.GetValue(i, j);

            if (value == _noEdgeValue)
            {
                throw new ArgumentException("L'arc n'existe pas.");
            }

            return value;
        }

        /* Affecte le poids l'arc allant du sommet nommé `sourceName` au sommet nommé `destinationName` à `weight` 
         * Si le graphe n'est pas orienté, affecte le même poids à l'arc inverse
         * Lève une ArgumentException si un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         */
        public void SetEdgeWeight(string sourceName, string destinationName, float weight)
        {
            if (!_vertexIndices.ContainsKey(sourceName) || !_vertexIndices.ContainsKey(destinationName))
            {
                throw new ArgumentException("Un des sommets n'existe pas.");
            }

            int i = _vertexIndices[sourceName];
            int j = _vertexIndices[destinationName];

            // Mise à jour du poids
            _matrix.SetValue(i, j, weight);

            if (!_directed)
            {
                _matrix.SetValue(j, i, weight);
            }
        }
    }
}
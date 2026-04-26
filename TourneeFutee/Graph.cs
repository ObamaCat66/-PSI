using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    public class Graph
    {
        // --- Champs privés ---
        private Matrix _matrix;
        private bool _directed;
        private float _noEdgeValue;
        private Dictionary<string, int> _vertexIndices;
        private List<string> _vertexNames;
        private List<float> _vertexValues;

        // --- Propriétés publiques (Corrigées pour les tests) ---

        // Indique si le graphe est orienté (utilisé par PersistanceTests)
        public bool IsOriented => _directed;

        // Alias souvent utilisé
        public bool Directed => _directed;

        // Permet aux tests d'accéder à la matrice d'adjacence pour vérifier les poids
        public Matrix AdjacencyMatrix => _matrix;

        // Liste des noms des sommets (utilisé par PersistanceTests)
        public IReadOnlyList<string> Vertices => _vertexNames;

        // Ordre du graphe (nombre de sommets)
        public int Order => _vertexNames.Count;

        // --- Constructeur ---

        public Graph(bool directed, float noEdgeValue = 0)
        {
            _directed = directed;
            _noEdgeValue = noEdgeValue;

            // Initialisation d'une matrice vide
            _matrix = new Matrix(0, 0, noEdgeValue);

            _vertexIndices = new Dictionary<string, int>();
            _vertexNames = new List<string>();
            _vertexValues = new List<float>();
        }

        // --- Gestion des sommets ---

        public void AddVertex(string name, float value = 0)
        {
            if (_vertexIndices.ContainsKey(name))
            {
                throw new ArgumentException($"Le sommet '{name}' existe déjà.");
            }

            int newIndex = Order;

            _vertexIndices[name] = newIndex;
            _vertexNames.Add(name);
            _vertexValues.Add(value);

            // Mise à jour de la matrice
            _matrix.AddRow(newIndex);
            _matrix.AddColumn(newIndex);
        }

        public void RemoveVertex(string name)
        {
            if (!_vertexIndices.ContainsKey(name))
            {
                throw new ArgumentException($"Le sommet '{name}' n'existe pas.");
            }

            int index = _vertexIndices[name];

            _matrix.RemoveRow(index);
            _matrix.RemoveColumn(index);

            _vertexNames.RemoveAt(index);
            _vertexValues.RemoveAt(index);

            // Reconstruction des indices après décalage
            _vertexIndices.Clear();
            for (int i = 0; i < _vertexNames.Count; i++)
            {
                _vertexIndices[_vertexNames[i]] = i;
            }
        }

        public float GetVertexValue(string name)
        {
            if (!_vertexIndices.ContainsKey(name)) throw new ArgumentException("Sommet introuvable.");
            return _vertexValues[_vertexIndices[name]];
        }

        public void SetVertexValue(string name, float value)
        {
            if (!_vertexIndices.ContainsKey(name)) throw new ArgumentException("Sommet introuvable.");
            _vertexValues[_vertexIndices[name]] = value;
        }

        // --- Gestion des arcs / arêtes ---

        public void AddEdge(string sourceName, string destinationName, float weight = 1)
        {
            if (!_vertexIndices.ContainsKey(sourceName) || !_vertexIndices.ContainsKey(destinationName))
            {
                throw new ArgumentException("Un des sommets n'existe pas.");
            }

            int i = _vertexIndices[sourceName];
            int j = _vertexIndices[destinationName];

            _matrix.SetValue(i, j, weight);

            if (!_directed)
            {
                _matrix.SetValue(j, i, weight);
            }
        }

        public void RemoveEdge(string sourceName, string destinationName)
        {
            if (!_vertexIndices.ContainsKey(sourceName) || !_vertexIndices.ContainsKey(destinationName))
            {
                throw new ArgumentException("Un des sommets n'existe pas.");
            }

            int i = _vertexIndices[sourceName];
            int j = _vertexIndices[destinationName];

            _matrix.SetValue(i, j, _noEdgeValue);

            if (!_directed)
            {
                _matrix.SetValue(j, i, _noEdgeValue);
            }
        }

        public float GetEdgeWeight(string sourceName, string destinationName)
        {
            if (!_vertexIndices.ContainsKey(sourceName) || !_vertexIndices.ContainsKey(destinationName))
            {
                throw new ArgumentException("Un des sommets n'existe pas.");
            }

            return _matrix.GetValue(_vertexIndices[sourceName], _vertexIndices[destinationName]);
        }

        public List<string> GetNeighbors(string vertexName)
        {
            if (!_vertexIndices.ContainsKey(vertexName)) throw new ArgumentException("Sommet introuvable.");

            List<string> neighbors = new List<string>();
            int i = _vertexIndices[vertexName];

            for (int j = 0; j < _matrix.NbColumns; j++)
            {
                if (_matrix.GetValue(i, j) != _noEdgeValue)
                {
                    neighbors.Add(_vertexNames[j]);
                }
            }
            return neighbors;
        }
    }
}
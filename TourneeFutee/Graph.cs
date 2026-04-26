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

        // --- Propriétés pour la compatibilité ---

        // Utilisé par GraphTests (Objectif 1)
        public int Order => _vertexNames.Count;

        // Utilisé par PersistanceTests (Objectif 3)
        public int VertexCount => _vertexNames.Count;

        // Utilisé par PersistanceTests
        public bool IsOriented => _directed;

        // Utilisé par GraphTests
        public bool Directed => _directed;

        // Requis pour la sauvegarde en base de données
        public Matrix AdjacencyMatrix => _matrix;

        // Liste des noms de sommets
        public IReadOnlyList<string> Vertices => _vertexNames;

        // --- Constructeur flexible ---

        
        public Graph(bool isOriented = true, float noEdgeValue = 0)
        {
            _directed = isOriented; // On remplit ton champ privé habituel
            _noEdgeValue = noEdgeValue;

            _matrix = new Matrix(0, 0, noEdgeValue);
            _vertexIndices = new Dictionary<string, int>();
            _vertexNames = new List<string>();
            _vertexValues = new List<float>();
        }


        // le constructeur ci-dessus fonctionne car C# fait la correspondance par position.

        // --- Gestion des sommets ---

        public bool ContainsVertex(string name)
        {
            return _vertexIndices.ContainsKey(name);
        }

        public void AddVertex(string name, float value = 0)
        {
            if (ContainsVertex(name))
            {
                throw new ArgumentException($"Le sommet '{name}' existe déjà.");
            }

            int newIndex = _vertexNames.Count;
            _vertexIndices[name] = newIndex;
            _vertexNames.Add(name);
            _vertexValues.Add(value);

            _matrix.AddRow(newIndex);
            _matrix.AddColumn(newIndex);
        }

        public void RemoveVertex(string name)
        {
            if (!ContainsVertex(name))
            {
                throw new ArgumentException($"Le sommet '{name}' n'existe pas.");
            }

            int index = _vertexIndices[name];
            _matrix.RemoveRow(index);
            _matrix.RemoveColumn(index);

            _vertexNames.RemoveAt(index);
            _vertexValues.RemoveAt(index);

            // Mise à jour du dictionnaire d'indices
            _vertexIndices.Clear();
            for (int i = 0; i < _vertexNames.Count; i++)
            {
                _vertexIndices[_vertexNames[i]] = i;
            }
        }

        public float GetVertexValue(string name)
        {
            if (!ContainsVertex(name)) throw new ArgumentException("Sommet introuvable.");
            return _vertexValues[_vertexIndices[name]];
        }

        public void SetVertexValue(string name, float value)
        {
            if (!ContainsVertex(name)) throw new ArgumentException("Sommet introuvable.");
            _vertexValues[_vertexIndices[name]] = value;
        }

        // --- Gestion des arcs ---

        // On ajoute 'bool strict = true' à la fin des paramètres
        public void AddEdge(string sourceName, string destinationName, float weight = 1, bool strict = true)
        {
            if (!ContainsVertex(sourceName) || !ContainsVertex(destinationName))
            {
                throw new ArgumentException("Un des sommets n'existe pas.");
            }

            int i = _vertexIndices[sourceName];
            int j = _vertexIndices[destinationName];

            // Si l'arc existe déjà dans la matrice
            if (_matrix.GetValue(i, j) != _noEdgeValue)
            {
                // MODE STRICT (Par défaut, utilisé par GraphTests)
                if (strict)
                {
                    throw new ArgumentException("L'arc existe déjà.");
                }

                // MODE SOUPLE (Utilisé par la Persistance)
                if (_matrix.GetValue(i, j) != weight)
                {
                    throw new ArgumentException("L'arc existe déjà avec un poids différent.");
                }
                return; // On sort sans erreur si le poids est le même
            }

            _matrix.SetValue(i, j, weight);

            if (!_directed)
            {
                _matrix.SetValue(j, i, weight);
            }
        }

        public void RemoveEdge(string sourceName, string destinationName)
        {
            if (!ContainsVertex(sourceName) || !ContainsVertex(destinationName))
            {
                throw new ArgumentException("Un des sommets n'existe pas.");
            }

            int i = _vertexIndices[sourceName];
            int j = _vertexIndices[destinationName];

            if (_matrix.GetValue(i, j) == _noEdgeValue)
            {
                throw new ArgumentException("L'arc n'existe pas.");
            }

            _matrix.SetValue(i, j, _noEdgeValue);
            if (!_directed) _matrix.SetValue(j, i, _noEdgeValue);
        }

        public float GetEdgeWeight(string sourceName, string destinationName)
        {
            if (!ContainsVertex(sourceName) || !ContainsVertex(destinationName))
            {
                throw new ArgumentException("Un des sommets n'existe pas.");
            }

            float w = _matrix.GetValue(_vertexIndices[sourceName], _vertexIndices[destinationName]);
            if (w == _noEdgeValue) throw new ArgumentException("L'arc n'existe pas.");
            return w;
        }

        public void SetEdgeWeight(string sourceName, string destinationName, float weight)
        {
            if (!ContainsVertex(sourceName) || !ContainsVertex(destinationName))
            {
                throw new ArgumentException("Un des sommets n'existe pas.");
            }

            _matrix.SetValue(_vertexIndices[sourceName], _vertexIndices[destinationName], weight);
            if (!_directed) _matrix.SetValue(_vertexIndices[destinationName], _vertexIndices[sourceName], weight);
        }

        public List<string> GetNeighbors(string vertexName)
        {
            if (!ContainsVertex(vertexName)) throw new ArgumentException("Sommet introuvable.");

            List<string> neighbors = new List<string>();
            int i = _vertexIndices[vertexName];
            for (int j = 0; j < _matrix.NbColumns; j++)
            {
                if (_matrix.GetValue(i, j) != _noEdgeValue)
                    neighbors.Add(_vertexNames[j]);
            }
            return neighbors;
        }
    }
}
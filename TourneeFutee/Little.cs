using System;
using System.Collections.Generic;
using System.Reflection;

namespace TourneeFutee
{
    // Résout le problème de voyageur de commerce défini par le graphe `graph`
    // en utilisant l'algorithme de Little
    public class Little
    {
        // TODO : ajouter tous les attributs que vous jugerez pertinents 
        private Graph _graph;
        private List<string> _vertexNames;
        private float _bestCost;
        private int[] _bestOrder;

        // Instancie le planificateur en spécifiant le graphe modélisant un problème de voyageur de commerce
        public Little(Graph graph)
        {
            // TODO : implémenter
            _graph = graph;
            _vertexNames = GetVertexNames(graph);
            _bestCost = float.PositiveInfinity;
            _bestOrder = new int[_vertexNames.Count];
        }

        // Trouve la tournée optimale dans le graphe `this.graph`
        // (c'est à dire le cycle hamiltonien de plus faible coût)
        public Tour ComputeOptimalTour()
        {
            // TODO : implémenter
            int nbCities = _vertexNames.Count;

            if (nbCities == 0)
            {
                return new Tour();
            }

            bool[] used = new bool[nbCities];
            int[] currentOrder = new int[nbCities];

            used[0] = true;
            currentOrder[0] = 0;

            SearchBestTour(position: 1, currentCost: 0.0f, used: used, currentOrder: currentOrder);

            if (float.IsPositiveInfinity(_bestCost))
            {
                return new Tour();
            }

            List<(string source, string destination)> segments = new List<(string source, string destination)>();

            for (int i = 0; i < nbCities - 1; i++)
            {
                string source = _vertexNames[_bestOrder[i]];
                string destination = _vertexNames[_bestOrder[i + 1]];
                segments.Add((source, destination));
            }

            segments.Add((_vertexNames[_bestOrder[nbCities - 1]], _vertexNames[_bestOrder[0]]));

            return new Tour(segments, _bestCost);
        }

        // --- Méthodes utilitaires réalisant des étapes de l'algorithme de Little


        // Réduit la matrice `m` et revoie la valeur totale de la réduction
        // Après appel à cette méthode, la matrice `m` est *modifiée*.
        public static float ReduceMatrix(Matrix m)
        {
            // TODO : implémenter
            float totalReduction = 0.0f;

            // Réduction des lignes
            for (int i = 0; i < m.NbRows; i++)
            {
                float minValue = float.PositiveInfinity;

                for (int j = 0; j < m.NbColumns; j++)
                {
                    float value = m.GetValue(i, j);
                    if (!float.IsPositiveInfinity(value) && value < minValue)
                    {
                        minValue = value;
                    }
                }

                if (!float.IsPositiveInfinity(minValue) && minValue > 0.0f)
                {
                    for (int j = 0; j < m.NbColumns; j++)
                    {
                        float value = m.GetValue(i, j);
                        if (!float.IsPositiveInfinity(value))
                        {
                            m.SetValue(i, j, value - minValue);
                        }
                    }

                    totalReduction += minValue;
                }
            }

            // Réduction des colonnes
            for (int j = 0; j < m.NbColumns; j++)
            {
                float minValue = float.PositiveInfinity;

                for (int i = 0; i < m.NbRows; i++)
                {
                    float value = m.GetValue(i, j);
                    if (!float.IsPositiveInfinity(value) && value < minValue)
                    {
                        minValue = value;
                    }
                }

                if (!float.IsPositiveInfinity(minValue) && minValue > 0.0f)
                {
                    for (int i = 0; i < m.NbRows; i++)
                    {
                        float value = m.GetValue(i, j);
                        if (!float.IsPositiveInfinity(value))
                        {
                            m.SetValue(i, j, value - minValue);
                        }
                    }

                    totalReduction += minValue;
                }
            }

            return totalReduction;
        }

        // Renvoie le regret de valeur maximale dans la matrice de coûts `m` sous la forme d'un tuple `(int i, int j, float value)`
        // où `i`, `j`, et `value` contiennent respectivement la ligne, la colonne et la valeur du regret maximale
        public static (int i, int j, float value) GetMaxRegret(Matrix m)
        {
            // TODO : implémenter
            int bestI = 0;
            int bestJ = 0;
            float bestRegret = float.NegativeInfinity;

            for (int i = 0; i < m.NbRows; i++)
            {
                for (int j = 0; j < m.NbColumns; j++)
                {
                    float value = m.GetValue(i, j);

                    if (value == 0.0f)
                    {
                        float rowMin = float.PositiveInfinity;
                        float columnMin = float.PositiveInfinity;

                        // minimum de la ligne i en excluant la colonne j
                        for (int k = 0; k < m.NbColumns; k++)
                        {
                            if (k != j)
                            {
                                float rowValue = m.GetValue(i, k);
                                if (!float.IsPositiveInfinity(rowValue) && rowValue < rowMin)
                                {
                                    rowMin = rowValue;
                                }
                            }
                        }

                        // minimum de la colonne j en excluant la ligne i
                        for (int k = 0; k < m.NbRows; k++)
                        {
                            if (k != i)
                            {
                                float columnValue = m.GetValue(k, j);
                                if (!float.IsPositiveInfinity(columnValue) && columnValue < columnMin)
                                {
                                    columnMin = columnValue;
                                }
                            }
                        }

                        if (float.IsPositiveInfinity(rowMin))
                        {
                            rowMin = 0.0f;
                        }

                        if (float.IsPositiveInfinity(columnMin))
                        {
                            columnMin = 0.0f;
                        }

                        float regret = rowMin + columnMin;

                        if (regret > bestRegret)
                        {
                            bestRegret = regret;
                            bestI = i;
                            bestJ = j;
                        }
                    }
                }
            }

            if (float.IsNegativeInfinity(bestRegret))
            {
                bestRegret = 0.0f;
            }

            return (bestI, bestJ, bestRegret);

        }

        /* Renvoie vrai si le segment `segment` est un trajet parasite, c'est-à-dire s'il ferme prématurément la tournée incluant les trajets contenus dans `includedSegments`
         * Une tournée est incomplète si elle visite un nombre de villes inférieur à `nbCities`
         */
        public static bool IsForbiddenSegment((string source, string destination) segment, List<(string source, string destination)> includedSegments, int nbCities)
        {

            // TODO : implémenter

            // On construit l'application "source -> destination" associée aux segments déjà inclus
            Dictionary<string, string> nextVertex = new Dictionary<string, string>();

            for (int i = 0; i < includedSegments.Count; i++)
            {
                string source = includedSegments[i].source;
                string destination = includedSegments[i].destination;

                if (!nextVertex.ContainsKey(source))
                {
                    nextVertex.Add(source, destination);
                }
            }

            // Ajouter `segment` ferme prématurément une tournée s'il existe déjà un chemin
            // allant de `segment.destination` vers `segment.source`.
            // Dans ce cas, l'ajout de `segment.source -> segment.destination` crée un cycle.
            string currentVertex = segment.destination;
            int chainLength = 1; // on comptera `segment` lui-même

            List<string> visited = new List<string>();
            visited.Add(segment.source);
            visited.Add(segment.destination);

            while (nextVertex.ContainsKey(currentVertex))
            {
                currentVertex = nextVertex[currentVertex];
                chainLength++;

                if (currentVertex == segment.source)
                {
                    return chainLength < nbCities;
                }

                // sécurité contre toute boucle anormale dans les données
                bool alreadyVisited = false;
                for (int i = 0; i < visited.Count; i++)
                {
                    if (visited[i] == currentVertex)
                    {
                        alreadyVisited = true;
                        break;
                    }
                }

                if (alreadyVisited)
                {
                    break;
                }

                visited.Add(currentVertex);
            }

            return false;
        }

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 

        private void SearchBestTour(int position, float currentCost, bool[] used, int[] currentOrder)
        {
            int nbCities = _vertexNames.Count;

            if (currentCost >= _bestCost)
            {
                return;
            }

            if (position == nbCities)
            {
                float lastEdgeCost = GetEdgeWeightSafe(_vertexNames[currentOrder[nbCities - 1]], _vertexNames[currentOrder[0]]);

                if (float.IsPositiveInfinity(lastEdgeCost))
                {
                    return;
                }

                float totalCost = currentCost + lastEdgeCost;

                if (totalCost < _bestCost)
                {
                    _bestCost = totalCost;

                    for (int i = 0; i < nbCities; i++)
                    {
                        _bestOrder[i] = currentOrder[i];
                    }
                }

                return;
            }

            int previousVertexIndex = currentOrder[position - 1];

            for (int candidate = 1; candidate < nbCities; candidate++)
            {
                if (!used[candidate])
                {
                    float edgeCost = GetEdgeWeightSafe(_vertexNames[previousVertexIndex], _vertexNames[candidate]);

                    if (!float.IsPositiveInfinity(edgeCost))
                    {
                        used[candidate] = true;
                        currentOrder[position] = candidate;

                        SearchBestTour(position + 1, currentCost + edgeCost, used, currentOrder);

                        used[candidate] = false;
                    }
                }
            }
        }

        private float GetEdgeWeightSafe(string source, string destination)
        {
            try
            {
                return _graph.GetEdgeWeight(source, destination);
            }
            catch (ArgumentException)
            {
                return float.PositiveInfinity;
            }
        }

        private static List<string> GetVertexNames(Graph graph)
        {
            FieldInfo field = typeof(Graph).GetField("_vertexNames", BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                throw new InvalidOperationException("Impossible de récupérer la liste des sommets du graphe.");
            }

            object value = field.GetValue(graph);

            List<string> names = value as List<string>;

            if (names == null)
            {
                throw new InvalidOperationException("Le champ interne des noms de sommets est introuvable.");
            }

            List<string> result = new List<string>();

            for (int i = 0; i < names.Count; i++)
            {
                result.Add(names[i]);
            }

            return result;
        }
    }
}
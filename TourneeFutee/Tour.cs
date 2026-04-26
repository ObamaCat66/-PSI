using System;
using System.Collections.Generic;


namespace TourneeFutee
{

    // Modélise une tournée dans le cadre du problème du voyageur de commerce
    public class Tour
    {
        // TODO : ajouter tous les attributs que vous jugerez pertinents 
        private float _cost;
        private List<(string source, string destination)> _segments;

        // propriétés

        // Coût total de la tournée
        public float Cost
        {
            get { return _cost; }    // TODO : implémenter
        }

        // Nombre de trajets dans la tournée
        public int NbSegments
        {
            get { return _segments.Count; }    // TODO : implémenter
        }

        public Tour()
        {
            _cost = 0.0f;
            _segments = new List<(string source, string destination)>();
        }

        public Tour(List<(string source, string destination)> segments, float cost)
        {
            _cost = cost;
            _segments = new List<(string source, string destination)>();

            for (int i = 0; i < segments.Count; i++)
            {
                _segments.Add(segments[i]);
            }
        }

        // Renvoie vrai si la tournée contient le trajet `source`->`destination`
        public bool ContainsSegment((string source, string destination) segment)
        {
            for (int i = 0; i < _segments.Count; i++)
            {
                if (_segments[i].source == segment.source && _segments[i].destination == segment.destination)
                {
                    return true;
                }
            }

            return false;   // TODO : implémenter 
        }


        // Affiche les informations sur la tournée : coût total et trajets
        public void Print()
        {
            Console.WriteLine("Coût total : " + _cost);

            Console.Write("Trajets : ");

            for (int i = 0; i < _segments.Count; i++)
            {
                Console.Write(_segments[i].source + "->" + _segments[i].destination);

                if (i < _segments.Count - 1)
                {
                    Console.Write(" ; ");
                }
            }

            Console.WriteLine();
        }

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 
        // Propriété utilisée par les tests
        public float TotalCost => _cost;

        // Propriété qui transforme les segments (A->C, C->F) en liste de sommets (A, C, F)
        public List<string> Vertices
        {
            get
            {
                var list = new List<string>();
                if (_segments.Count == 0) return list;
                foreach (var seg in _segments) list.Add(seg.source);
                list.Add(_segments[_segments.Count - 1].destination); // Le dernier retour au point de départ
                return list;
            }
        }

        // Constructeur attendu par les tests unitaires (Test 6)
        public Tour(List<string> vertexSequence, float cost)
        {
            _cost = cost;
            _segments = new List<(string source, string destination)>();
            for (int i = 0; i < vertexSequence.Count - 1; i++)
            {
                _segments.Add((vertexSequence[i], vertexSequence[i + 1]));
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace TourneeFutee
{
    public class Matrix
    {
        // On utilise une liste de listes pour gérer facilement l'ajout/suppression de lignes
        private List<List<float>> _data;

        // On stocke le nombre de colonnes séparément pour gérer le cas où il y a 0 lignes
        private int _nbColumns;

        private float _defaultValue;

        /* Crée une matrice de dimensions `nbRows` x `nbColums`.
         * Toutes les cases de cette matrice sont remplies avec `defaultValue`.
         * Lève une ArgumentOutOfRangeException si une des dimensions est négative
         */
        public Matrix(int nbRows = 0, int nbColumns = 0, float defaultValue = 0)
        {
            if (nbRows < 0 || nbColumns < 0)
            {
                throw new ArgumentOutOfRangeException("Les dimensions ne peuvent pas être négatives.");
            }

            _defaultValue = defaultValue;
            _nbColumns = nbColumns;
            _data = new List<List<float>>(nbRows);

            for (int i = 0; i < nbRows; i++)
            {
                // Création d'une ligne remplie avec la valeur par défaut
                List<float> row = new List<float>();
                for (int j = 0; j < nbColumns; j++)
                {
                    row.Add(defaultValue);
                }
                _data.Add(row);
            }
        }

        // Propriété : valeur par défaut utilisée pour remplir les nouvelles cases
        // Lecture seule
        public float DefaultValue
        {
            get { return _defaultValue; }
        }

        // Propriété : nombre de lignes
        // Lecture seule
        public int NbRows
        {
            get { return _data.Count; }
        }

        // Propriété : nombre de colonnes
        // Lecture seule
        public int NbColumns
        {
            get { return _nbColumns; }
        }

        /* Insère une ligne à l'indice `i`. Décale les lignes suivantes vers le bas.
         * Toutes les cases de la nouvelle ligne contiennent DefaultValue.
         * Si `i` = NbRows, insère une ligne en fin de matrice
         * Lève une ArgumentOutOfRangeException si `i` est en dehors des indices valides
         */
        public void AddRow(int i)
        {
            if (i < 0 || i > NbRows) // > NbRows est autorisé pour l'ajout à la fin
            {
                throw new ArgumentOutOfRangeException(nameof(i));
            }

            // Création de la nouvelle ligne
            List<float> newRow = new List<float>();
            for (int k = 0; k < _nbColumns; k++)
            {
                newRow.Add(_defaultValue);
            }

            _data.Insert(i, newRow);
        }

        /* Insère une colonne à l'indice `j`. Décale les colonnes suivantes vers la droite.
         * Toutes les cases de la nouvelle ligne contiennent DefaultValue.
         * Si `j` = NbColums, insère une colonne en fin de matrice
         * Lève une ArgumentOutOfRangeException si `j` est en dehors des indices valides
         */
        public void AddColumn(int j)
        {
            if (j < 0 || j > _nbColumns) // > NbColumns est autorisé pour l'ajout à la fin
            {
                throw new ArgumentOutOfRangeException(nameof(j));
            }

            // On ajoute la valeur par défaut à l'indice j pour chaque ligne existante
            foreach (var row in _data)
            {
                row.Insert(j, _defaultValue);
            }

            _nbColumns++;
        }

        // Supprime la ligne à l'indice `i`. Décale les lignes suivantes vers le haut.
        // Lève une ArgumentOutOfRangeException si `i` est en dehors des indices valides
        public void RemoveRow(int i)
        {
            if (i < 0 || i >= NbRows)
            {
                throw new ArgumentOutOfRangeException(nameof(i));
            }

            _data.RemoveAt(i);
        }

        // Supprime la colonne à l'indice `j`. Décale les colonnes suivantes vers la gauche.
        // Lève une ArgumentOutOfRangeException si `j` est en dehors des indices valides
        public void RemoveColumn(int j)
        {
            if (j < 0 || j >= _nbColumns)
            {
                throw new ArgumentOutOfRangeException(nameof(j));
            }

            foreach (var row in _data)
            {
                row.RemoveAt(j);
            }

            _nbColumns--;
        }

        // Renvoie la valeur à la ligne `i` et colonne `j`
        // Lève une ArgumentOutOfRangeException si `i` ou `j` est en dehors des indices valides
        public float GetValue(int i, int j)
        {
            if (i < 0 || i >= NbRows || j < 0 || j >= _nbColumns)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _data[i][j];
        }

        // Affecte la valeur à la ligne `i` et colonne `j` à `v`
        // Lève une ArgumentOutOfRangeException si `i` ou `j` est en dehors des indices valides
        public void SetValue(int i, int j, float v)
        {
            if (i < 0 || i >= NbRows || j < 0 || j >= _nbColumns)
            {
                throw new ArgumentOutOfRangeException();
            }

            _data[i][j] = v;
        }

        // Affiche la matrice
        public void Print()
        {
            for (int i = 0; i < NbRows; i++)
            {
                for (int j = 0; j < NbColumns; j++)
                {
                    Console.Write(_data[i][j] + "\t");
                }
                Console.WriteLine();
            }
        }
    }
}
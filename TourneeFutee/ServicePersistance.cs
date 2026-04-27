using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace TourneeFutee 
/* Veuillez lire le README : les instructions (probablement évidentes pour vous) pour faire fonctionner les 7 derniers tests y sont indiquées. Je les remets ici au cas où.

Bonjour,
Pour pouvoir exécuter les 7 tests de persistance, il vous faudra modifier la ligne 14 de ServicePersistance et mettre votre propre mot de passe à la place de celui déjà écrit après Pwd.
Ensuite, il faut ouvrir notre fichier SQL, présent dans le dossier de la solution, avec MySQL, puis le lancer une fois en cliquant sur l’éclair.
Après cela, tous les tests compileront sans problème.
*/
{
    public class ServicePersistance
    {
        private readonly string _connectionString;

        public ServicePersistance(string serverIp, string dbname, string user, string pwd)
        {
            // Vérification du nom de la base (Database) et de l'utilisateur (Uid), mettre soin propre mot de passe
            _connectionString = "Server=localhost;Database=tourneefutee;Uid=root;Pwd=Philipot2602;";//<=== Mot de passe a remplacer ici
            using (var conn = OpenConnection()) { /* Test connexion */ }
        }

        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        // --- GESTION DES GRAPHES ---

        public uint SaveGraph(Graph g)
        {
            using (var conn = OpenConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    // 1. Graphe
                    var cmdG = new MySqlCommand("INSERT INTO Graphe (is_oriented) VALUES (@dir); SELECT LAST_INSERT_ID();", conn, trans);
                    cmdG.Parameters.AddWithValue("@dir", g.Directed);
                    uint graphId = Convert.ToUInt32(cmdG.ExecuteScalar());

                    // 2. Sommets
                    var vertexDbIds = new Dictionary<string, uint>();
                    foreach (string name in g.Vertices)
                    {
                        var cmdS = new MySqlCommand("INSERT INTO Sommet (nom, valeur, graphe_id) VALUES (@n, @v, @gid); SELECT LAST_INSERT_ID();", conn, trans);
                        cmdS.Parameters.AddWithValue("@n", name);
                        cmdS.Parameters.AddWithValue("@v", g.GetVertexValue(name));
                        cmdS.Parameters.AddWithValue("@gid", graphId);
                        vertexDbIds[name] = Convert.ToUInt32(cmdS.ExecuteScalar());
                    }

                    // 3. Arcs
                    foreach (string src in g.Vertices)
                    {
                        foreach (string dst in g.Vertices)
                        {
                            try
                            {
                                float w = g.GetEdgeWeight(src, dst);
                                var cmdA = new MySqlCommand("INSERT INTO Arc (sommet_source_id, sommet_dest_id, poids, graphe_id) VALUES (@s, @d, @p, @gid);", conn, trans);
                                cmdA.Parameters.AddWithValue("@s", vertexDbIds[src]);
                                cmdA.Parameters.AddWithValue("@d", vertexDbIds[dst]);
                                cmdA.Parameters.AddWithValue("@p", w);
                                cmdA.Parameters.AddWithValue("@gid", graphId);
                                cmdA.ExecuteNonQuery();
                            }
                            catch (ArgumentException) { /* L'arc n'existe pas dans ta Matrix */ }
                        }
                    }
                    trans.Commit();
                    return graphId;
                }
                catch { trans.Rollback(); throw; }
            }
        }

        public Graph LoadGraph(uint id)
        {
            using (var conn = OpenConnection())
            {
                bool directed;
                using (var cmd = new MySqlCommand("SELECT is_oriented FROM Graphe WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    directed = Convert.ToBoolean(cmd.ExecuteScalar());
                }

                // Initialisation avec la classe Graph (on met 0 ou +Infini en noEdgeValue selon l'usage)
                Graph g = new Graph(directed, float.PositiveInfinity);
                var idToName = new Dictionary<uint, string>();

                using (var cmd = new MySqlCommand("SELECT id, nom, valeur FROM Sommet WHERE graphe_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string n = r.GetString("nom");
                            g.AddVertex(n, r.GetFloat("valeur"));
                            idToName[r.GetUInt32("id")] = n;
                        }
                    }
                }

                using (var cmd = new MySqlCommand("SELECT sommet_source_id, sommet_dest_id, poids FROM Arc WHERE graphe_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            // Le "false" à la fin correspond au paramètre 'strict' de notre nouvelle méthode AddEdge
                            g.AddEdge(idToName[r.GetUInt32("sommet_source_id")], idToName[r.GetUInt32("sommet_dest_id")], r.GetFloat("poids"), false);
                    }
                }
                return g;
            }
        }

        // --- GESTION DES TOURNEES ---

        public uint SaveTour(uint graphId, Tour t)
        {
            using (var conn = OpenConnection())
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    var cmdT = new MySqlCommand("INSERT INTO Tournee (cout_total, graphe_id) VALUES (@c, @gid); SELECT LAST_INSERT_ID();", conn, trans);
                    cmdT.Parameters.AddWithValue("@c", t.TotalCost);
                    cmdT.Parameters.AddWithValue("@gid", graphId);
                    uint tourId = Convert.ToUInt32(cmdT.ExecuteScalar());

                    // Mapping Nom -> ID BdD pour les clés étrangères
                    var nameToId = new Dictionary<string, uint>();
                    using (var cmdS = new MySqlCommand("SELECT id, nom FROM Sommet WHERE graphe_id=@gid", conn, trans))
                    {
                        cmdS.Parameters.AddWithValue("@gid", graphId);
                        using (var r = cmdS.ExecuteReader())
                            while (r.Read()) nameToId[r.GetString("nom")] = r.GetUInt32("id");
                    }

                    // Sauvegarde des étapes (utilise la propriété Vertices qu'on a ajoutée à Tour)
                    var verts = t.Vertices;
                    for (int i = 0; i < verts.Count; i++)
                    {
                        var cmdE = new MySqlCommand("INSERT INTO EtapeTournee (tournee_id, numero_ordre, sommet_id) VALUES (@tid, @o, @sid);", conn, trans);
                        cmdE.Parameters.AddWithValue("@tid", tourId);
                        cmdE.Parameters.AddWithValue("@o", i + 1);
                        cmdE.Parameters.AddWithValue("@sid", nameToId[verts[i]]);
                        cmdE.ExecuteNonQuery();
                    }

                    trans.Commit();
                    return tourId;
                }
                catch { trans.Rollback(); throw; }
            }
        }

        public Tour LoadTour(uint id)
        {
            using (var conn = OpenConnection())
            {
                float cost;
                using (var cmd = new MySqlCommand("SELECT cout_total FROM Tournee WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cost = Convert.ToSingle(cmd.ExecuteScalar());
                }

                var sequence = new List<string>();
                using (var cmd = new MySqlCommand("SELECT s.nom FROM EtapeTournee e JOIN Sommet s ON e.sommet_id=s.id WHERE e.tournee_id=@id ORDER BY e.numero_ordre", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) sequence.Add(r.GetString("nom"));
                }

                // Utilise le constructeur par séquence qu'on a ajouté à Tour
                return new Tour(sequence, cost);
            }
        }
    }
}
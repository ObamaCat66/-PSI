-- =============================================================================
-- PSI 2025-2026 – Objectif 3 : Base de données
-- Script d'initialisation de la base de données TourneeFutee
--
-- Instructions :
--   1. Créez la base de données avec : CREATE DATABASE tourneefutee;
--   2. Sélectionnez-la avec      : USE tourneefutee;
--   3. Exécutez ce script complet pour créer toutes les tables.
--
-- TODO : compléter les parties marquées "TODO" ci-dessous.
-- =============================================================================
CREATE DATABASE IF NOT EXISTS tourneefutee;
USE tourneefutee;
-- Supprimer les tables dans l'ordre inverse des dépendances (pour réinitialiser)
DROP TABLE IF EXISTS EtapeTournee;
DROP TABLE IF EXISTS Tournee;
DROP TABLE IF EXISTS Arc;
DROP TABLE IF EXISTS Sommet;
DROP TABLE IF EXISTS Graphe;

-- =============================================================================
-- Table : Graphe
-- Représente un graphe (orienté ou non).
-- =============================================================================

CREATE TABLE Graphe (
    id           INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    is_oriented  TINYINT(1)      NOT NULL DEFAULT 0, -- Harmonisé avec le code C#
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE Sommet (
    id          INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    graphe_id   INT UNSIGNED    NOT NULL,
    nom         VARCHAR(50)     NOT NULL,
    valeur      FLOAT           NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (graphe_id) REFERENCES Graphe(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE Arc (
    id              INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    graphe_id       INT UNSIGNED    NOT NULL,
    sommet_source_id INT UNSIGNED    NOT NULL, -- Changé pour correspondre au code
    sommet_dest_id   INT UNSIGNED    NOT NULL, -- Changé pour correspondre au code
    poids           FLOAT           NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (graphe_id)     REFERENCES Graphe(id) ON DELETE CASCADE,
    FOREIGN KEY (sommet_source_id) REFERENCES Sommet(id) ON DELETE CASCADE,
    FOREIGN KEY (sommet_dest_id)   REFERENCES Sommet(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE Tournee (
    id          INT UNSIGNED    NOT NULL AUTO_INCREMENT,
    graphe_id   INT UNSIGNED    NOT NULL,
    cout_total  FLOAT           NOT NULL,
    PRIMARY KEY (id),
    FOREIGN KEY (graphe_id) REFERENCES Graphe(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE EtapeTournee (
    tournee_id      INT UNSIGNED    NOT NULL,
    numero_ordre    INT UNSIGNED    NOT NULL,
    sommet_id       INT UNSIGNED    NOT NULL,
    PRIMARY KEY (tournee_id, numero_ordre),
    FOREIGN KEY (tournee_id) REFERENCES Tournee(id) ON DELETE CASCADE,
    FOREIGN KEY (sommet_id)  REFERENCES Sommet(id)  ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- =============================================================================
-- Vérification : afficher les tables créées
-- =============================================================================
SHOW TABLES;

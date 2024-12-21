# Système de Gestion des Capteurs API (Tâche d'Évaluation)
Ce projet est une API RESTful développée avec .NET 8 pour la gestion des capteurs. L'application comprend deux versions d'API, chacune interagissant avec différentes bases de données et offrant des fonctionnalités uniques.
## Fonctionnalités et Implémentation Principales :
### Conception de l'API :
• Création des endpoints pour ajouter, récupérer, mettre à jour et supprimer des capteurs.

• L'API permet des opérations complètes de gestion des données des capteurs.

### Intégration de la Base de Données :
• API v1 interagit avec une base de données SQL Server à l'aide de Entity Framework Core.

• API v2 est construite pour interagir avec une base de données PostgreSQL, montrant la flexibilité de l'API avec différents systèmes de bases de données.

### Authentification et Sécurité :
Authentification basée sur JWT configurée pour sécuriser les endpoints de l'API et garantir que seuls les utilisateurs autorisés puissent effectuer des opérations.

### Versioning de l'API :
Mise en place du versioning de l'API avec deux versions :

• v1 (SQL Server) : Fonctionnalités de base pour la gestion des capteurs.

• v2 (PostgreSQL) : Introduction d'une nouvelle fonctionnalité pour la gestion des tickets, améliorant les capacités de l'API tout en maintenant la compatibilité ascendante.

### Mise en Cache :
Implémentation de la mise en cache à l'aide de Redis pour améliorer les performances, en particulier pour les endpoints fréquemment accédés.

### Logging et Gestion des Erreurs :
• Utilisation de NLog pour un logging détaillé des erreurs, facilitant le suivi des problèmes en production.

• Codes d'erreur personnalisés et messages d'erreur détaillés inclus pour une meilleure détection des problèmes et une meilleure expérience utilisateur.

### Tests :
Utilisation de XUnit pour les tests unitaires, garantissant que les fonctionnalités principales de l'API sont testées et fonctionnent comme prévu.

### Documentation :
Utilisation de Swagger pour une documentation complète de l'API, offrant une interface facile à comprendre pour les utilisateurs et les développeurs.

### Déploiement :
Le projet a été déployé sur IIS sur un serveur Windows pour être accessible et testé en conditions réelles.

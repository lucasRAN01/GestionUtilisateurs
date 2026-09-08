# GestionUtilisateurs

Application mobile complète de **gestion des utilisateurs**, **100 % hors ligne**, développée en **C# / .NET MAUI** avec une architecture **MVVM**.

Fonctionne sur **Android** et **Windows**. Aucun serveur, aucune API : toutes les données sont stockées localement dans une base **SQLite**.

---

## Fonctionnalités

### Authentification locale
- Écran de connexion avec nom d'utilisateur / mot de passe
- Création du **premier compte administrateur** au premier démarrage
- Déconnexion
- Session locale conservée en mémoire
- Verrouillage automatique du compte après **5 échecs** consécutifs
- Gestion du statut actif / inactif

### Rôles et permissions
- Trois rôles pré-configurés : **Administrateur**, **Gestionnaire**, **Utilisateur**
- Permissions configurables par rôle (table de liaison `RolePermissions`)

### Tableau de bord administrateur
- Nombre total d'utilisateurs
- Utilisateurs **actifs** et **inactifs**
- Recherche et filtrage dans la liste

### Gestion des utilisateurs (CRUD)
- Nom, prénom, e-mail, téléphone, nom d'utilisateur, notes
- Attribution de rôle
- Activation / désactivation d'un compte
- Réinitialisation du mot de passe
- Suppression (protégée : pas de suppression du dernier admin)
- Validation des formulaires

### Base de données SQLite
- Tables : `Users`, `Roles`, `Permissions`, `AuditLogs` (+ table de liaison `RolePermissions`)
- Migrations Entity Framework Core
- Initialisation automatique de la base (rôles et permissions créés au démarrage)

### Historique (journal d'audit)
- Journalisation des connexions, déconnexions, échecs de connexion
- Créations, modifications, suppressions, changements de statut, réinitialisations, attributions de rôle
- Recherche dans l'historique

### Sécurité
- Mots de passe hachés avec **PBKDF2** (100 000 itérations, sel aléatoire, SHA-256) — non réversibles
- Comparaison en temps constant contre les attaques temporelles

---

## Prérequis

| Composant | Version |
|-----------|---------|
| [.NET SDK](https://dotnet.microsoft.com/fr-fr/download/dotnet/8.0) | 8.0.x (ou version LTS supérieure) |
| Workload **.NET MAUI** | Version alignée sur le SDK |

### Installation du workload MAUI

```bash
dotnet workload install maui
```

> Sur **Windows** pour cibler Android, installez au préalable :
> - **Visual Studio 2022** (option « Développement d'applications mobiles avec .NET ») **ou** le kit Android via le workload .NET.
> - Le **JDK** (Microsoft OpenJDK 17) et l'**Android SDK** sont fournis automatiquement par le workload MAUI.
> - Pour l'exécution sur Windows, la machine doit avoir les mises à jour Windows récentes (WinUI 3).

---

## Installation et exécution

### 1. Récupération

```bash
git clone <url-du-depot>
cd GestionUtilisateurs
```

### 2. Restauration des dépendances

```bash
dotnet restore GestionUtilisateurs.sln
```

### 3. Exécution sur Windows

```bash
dotnet build GestionUtilisateurs/GestionUtilisateurs.csproj -t:Run -f net8.0-windows10.0.19041.0
```

ou depuis Visual Studio : définir `GestionUtilisateurs` comme projet de démarrage, choisir la plateforme `Windows Machine`.

### 4. Exécution sur Android

```bash
dotnet build GestionUtilisateurs/GestionUtilisateurs.csproj -t:Run -f net8.0-android
```

ou depuis Visual Studio :
- Branché à un appareil physique (mode développeur activé) : `Android Local Device`.
- Avec un émulateur Android configuré.

### 5. Premier lancement

1. L'application détecte qu'aucun administrateur n'existe et ouvre l'écran **« Création du compte administrateur »**.
2. Renseignez le prénom, nom, e-mail, nom d'utilisateur et mot de passe.
3. Connectez-vous ensuite avec ces identifiants.
4. Vous arrivez sur le **tableau de bord** avec les statistiques.

### 6. Données de démonstration (facultatif)

Aucune donnée de démonstration n'est injectée par défaut pour ne pas polluer la base réelle.
Vous pouvez créer des comptes de test directement depuis l'interface (« Ajouter ») et les attribuer aux rôles.
Un CLI `dotnet` d'initialisation avec données de démo peut être ajouté dans `DatabaseInitializer` (voir commentaire dans le code).

---

## Architecture

```
GestionUtilisateurs/
├── GestionUtilisateurs/                  # Application MAUI (interface + navigation)
│   ├── App.xaml(.cs)                     # Racine de l'application
│   ├── AppShell.xaml(.cs)                # Shell + enregistrement des routes
│   ├── MauiProgram.cs                    # Injection de dépendances
│   ├── Models/                           # (vue directe des entités du domaine)
│   ├── Services/                         # NavigationService (abstraction Shell)
│   ├── ViewModels/                       # ViewModels + AsyncRelayCommand + base
│   ├── Views/                            # Pages XAML (.xaml + code-behind)
│   ├── Converters/                       # Convertisseurs de binding XAML
│   ├── Resources/                        # Styles, couleurs, icônes, splash
│   ├── Utilities/                        # Validation de formulaires
│   └── Platforms/                        # Android, Windows, ...
│
├── GestionUtilisateurs.DataAccess/       # Bibliothèque de données (testable)
│   ├── Models/                           # User, Role, Permission, AuditLog
│   ├── Data/AppDbContext.cs              # Contexte EF Core
│   ├── Migrations/                       # Migration initiale + snapshot
│   └── Services/                         # Auth, User, Role, Audit, Hash, Initializer
│
└── GestionUtilisateurs.Tests/            # Tests unitaires xUnit
```

### Injection de dépendances (`MauiProgram.cs`)

| Service | Rôle |
|---------|------|
| `IAppDbFactory` | Fabrique du `AppDbContext` (chemin SQLite) |
| `IDatabaseInitializer` | Crée/migre la base, insère rôles + permissions |
| `IPasswordHasher` | Hachage PBKDF2 |
| `IAuthService` | Connexion, session, verrouillage, journalisation |
| `IUserService` | CRUD, activation, rôles, réinitialisation |
| `IRoleService` | Liste des rôles |
| `IAuditLogger` / `IAuditService` | Écriture et lecture du journal |

Tous les ViewModels sont injectés dans les pages (`BindingContext`) ; la navigation passe par `INavigationService`.

---

## Base de données

Fichier : `GestionUtilisateurs.db` dans le dossier de données de l'application (`FileSystem.AppDataDirectory`).

La base est créée automatiquement au premier démarrage :

1. `Database.MigrateAsync()` applique les migrations (table `__EFMigrationsHistory`).
2. Les rôles (`Administrateur`, `Gestionnaire`, `Utilisateur`) et leurs permissions sont insérés si absents.

### Tables

| Table | Description |
|-------|-------------|
| `Users` | Utilisateurs, hachage du mot de passe, statut, verrouillage |
| `Roles` | Rôles |
| `Permissions` | Permissions configurables |
| `RolePermissions` | Liaison plusieurs-à-plusieurs rôle ↔ permission |
| `AuditLogs` | Journal des actions |

### Ajouter une migration

```bash
dotnet ef migrations add MaMigration \
  --project GestionUtilisateurs.DataAccess \
  --startup-project GestionUtilisateurs.DataAccess
```

### Supprimer / recréer la base (dev)

Supprimez simplement le fichier `GestionUtilisateurs.db` (dossier de données de l'app) puis relancez.

---

## Tests unitaires

Les tests ciblent la couche `GestionUtilisateurs.DataAccess` et utilisent une base **SQLite en mémoire**.

```bash
dotnet test GestionUtilisateurs.Tests/GestionUtilisateurs.Tests.csproj
```

Couverture :
- Hachage / vérification PBKDF2 (round-trip, mauvais mot de passe, sel aléatoire, Unicode)
- Authentification (succès, échecs, verrouillage, compte inactif, déconnexion)
- Gestion des utilisateurs (création, unicité, mise à jour, suppression, statut, rôles)
- Journal d'audit (écriture des actions, recherche)
- Validation des formulaires

---

## Sécurité

- **Mots de passe** : jamais stockés en clair. Format PBKDF2 : `$PBKDF2${itérations}${sel}${haché}`.
- **Attaques par force brute** : verrouillage automatique après 5 échecs consécutifs (10 min).
- **Énumération de comptes** : message identique pour « utilisateur inconnu » et « mot de passe incorrect », temps de réponse normalisé.
- **Confidentialité** : aucune communication réseau ; la base et le journal restent sur l'appareil.

---

## Résolution de problèmes

| Symptôme | Solution |
|----------|----------|
| `workload not found: maui` | `dotnet workload install maui-ios maui-android maui-windows` |
| Erreur JDK / Android SDK | `dotnet workload repair` puis réinstallez le workload MAUI |
| Build Android lent au premier coup | Normal : téléchargement des paquets et premières compilations |
| Base corrompue en dev | Supprimez `GestionUtilisateurs.db` et relancez l'application |
| `dotnet` n'est pas reconnu | Installez le SDK .NET 8 depuis dotnet.microsoft.com et redémarrez le terminal |

---

## Licence

Projet d'exemple / démonstration — libre d'utilisation.
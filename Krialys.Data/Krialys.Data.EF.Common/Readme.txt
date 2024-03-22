************************************************************************************************************************
**************************************** ENTITIES (Entity Framework) ***************************************************
************************************************************************************************************************

> ENTITIES a pour but de regrouper toutes les Entités dans un seul et unique projet de type librairie .NET Standard
  > Cette librairie est une dépendance qui est rattachée au projet APIUnivers (socle WebAPI/OData du moteur V2)
  > Cette librairie est un SDK redistribuable à un client/partenaire qui souhaiterait utiliser notre architecture

> Arborescence ENTITIES :
  > DBMS                = répertoire des entités (DbSet)
  > DBMS\MSO\Entities   = répertoire des entités liées au projet/database MSO     (namespace EFUnivers.MSO.Entity)
  > DBMS\STORE\Entities = répertoire des entités liées au projet/database DbStore (namespace EFUnivers.DbStore)
  > efpt.config.json    = fichier de configuration utilisé par EF Core PowerTools pour faire du reverse sur les BDD

*** SCAFFOLDING ***
> Pour générer le code des Entités : https://docs.microsoft.com/en-us/ef/core/managing-schemas/scaffolding?tabs=dotnet-core-cli
  > -- Installer EF CLI
  dotnet tool install --global dotnet-ef

  > -- Pour upgrader en 5.0
  dotnet tool update --global dotnet-ef

  > -- Scaffolding (chemin COMPLET/RELATIF pour le provider de BDD), remplacer 'STORE' par le nom de la BDD
  dotnet ef dbcontext scaffold "Data Source=../../ApiUnivers/App_Data/Database/db-Store.db3" --namespace Krialys.Data.EF.Store --context DbContextStore -o _tmpEntities -f Microsoft.EntityFrameworkCore.Sqlite --use-database-names --data-annotations

*** MIGRATION ***
> A lire : https://stackoverflow.com/questions/38705694/add-migration-with-different-assembly


> Dépendances de ENTITIES :
   > La seule dépendance qui doit persister est "Microsoft.EntityFrameworkCore" v3.1.4 ou supérieure
   > Bien faire attention lorsqu'on utilise EF Core Power Tools qui peut installer des librairies => à retirer

1 - Ajouter un script de migration pour un DbContext donné depuis le Package Manager en ligne de commandes :
   > PM> Add-Migration InitialDbStoreCreation -context DbStoreContext
   > Build started...
   > Build succeeded.
   > To undo this action, use Remove-Migration -context DbStoreContext

2 - Modifier les entités (modif. de propriétés), champ Comment de type text(255) dans la table Products :
   > PM> Add-Migration DbStoreContextAddComment -context DbStoreContext
   > Build started...
   > Build succeeded.
   > To undo this action, use Remove-Migration -context DbStoreContext

3 - Appliquer les migrations sur la base de données (modifications précédentes) ne va pas tronquer la table Products :
   > PM> Update-Database -context DbStoreContext
   > Build started...
   > Build succeeded.
   > Done.

4 - Appliquer les migrations sur la base de données de manière forcée (tronquera les tables ciblées) :
   > PM> Enable-Migrations -Force

> Références :
   > YouTube https://youtu.be/qkJ9keBmQWo => récap à partir de 2h10min de la vidéo
***********************************************************************
****** Mini documentation ParallelU Werker by GEG (25 Août 2020) ******
***********************************************************************

Ce projet nommé ParallelU est une solution Net Core 3 console utilisant Microsoft.NET.Sdk.Worker
Ce worker permet d'échanger via ApiUnivers via SignalR en consommant le endpoint nommé '/parallelhub'
Ce worker utilise le meme listener que ApiUnivers (par défaut il utilise le serveur Kestrel)

La communication se fait depuis ApiUnivers qui centralise les appels RESTFul et SignalR :

-> Le 'client' ParallelU est un fichier exécutable (Krialys.Orkestra.ParallelU.exe)
-> Le 'serveur' ParallelU est embarqué sous forme de service dans ApiUnivers et partage la même URL de base et le même port que ApiUnivers 

Enjoy !

Gérôme Guillemin
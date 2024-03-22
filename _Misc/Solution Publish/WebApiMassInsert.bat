@ECHO OFF

FOR /L %%A IN (1,1,100) DO (
  curl -X POST -H "Content-Type: application/json" -d "[{'iId':'0','Key':'Poiqsreaux','Name':'Bsdleus','Category':'Legumes','Quantity':40}, {'iId':'0','Key':'Poifreaux','Name':'Blefus','Category':'Legumes','Quantity':40}]" http://localhost:8000/api/v1/products/Create
)

:: https://docs.microsoft.com/en-us/odata/webapi/first-odata-api

:: http://localhost:8000/api/mso/v1/ttl_logs?$filter=startswith(TRA_CODE,%20%27DOCT%27)

:: http://localhost:8000/api/mso/v1/ttl_logs?$filter=TTL_FICHIER_SOURCE%20eq%20null

:: http://localhost:8000/api/mso/v1/ttl_logs?$orderby=ttl_id_log,%20ttl_info

Pause

:: curl -X POST -H "Content-Type: application/json" -d "[{'iId':'2','Key':'Poiqsreaux','Name':'Bsdleus','Category':'Legumes','Quantity':40}]" http://localhost:8000/api/store/v1/products
namespace Krialys.Data.EF.Etq;

/// <summary>
/// Arborescence d'étiquettes
/// </summary>
public class ArboEtiquettes
{
    /// <summary>
    /// Id étiquette
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Used to have a unique id independant from ID field
    /// </summary>
    public int ComputedId { get; set; }

    /// <summary>
    /// Niveau
    /// </summary>
    public int LEVEL { get; set; }

    /// <summary>
    /// Id étiquette parent
    /// </summary>
    public int? PARENTID { get; set; }

    /// <summary>
    /// Used to have an id relative to ComputedId field
    /// </summary>
    public int? ComputedParentId { get; set; }

    /// <summary>
    /// Code Etiquette
    /// </summary>
    public string TETQ_CODE { get; set; }

    /// <summary>
    /// Demande Id
    /// </summary>
    public int? DEMANDEID { get; set; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime TETQ_DATE_CREATION { get; set; }

    /// <summary>
    /// Description étiquette
    /// </summary>
    public string TETQ_DESC { get; set; }

    /// <summary>
    /// Libellé étiquette
    /// </summary>
    public string TETQ_LIB { get; set; }

    /// <summary>
    /// Libellé domaine
    /// </summary>
    public string TDOM_LIB { get; set; }

    /// <summary>
    /// Libellé périmètre
    /// </summary>
    public string TPRCP_LIB { get; set; }

    /// <summary>
    /// Identifies parent or children
    /// </summary>
    public string IMAGE_URL { get; set; }

    /// <summary>
    /// Une etiquette peut être en mode consommation ou en mode production
    /// </summary>
    public enum Mode
    {
        Consommation = 0, // LayoutOrientation.TopToBottom
        Production = 1,   // LayoutOrientation.BottomToTop
    }

    /// <summary>
    /// CONSOMMATION : l'etiquette a servi comme source pour d'autres etiquettes
    /// A présenter : du haut vers bas ou de gauche vers la droite
    /// </summary>
    /// <param name="etiquetteId"></param>
    /// <param name="level"></param>
    /// <returns>Raw Sql</returns>
    public static string Consommation(int etiquetteId, int level = 5)
        => @$"WITH RECURSIVE CONSOMMATION (LEVEL, TETQ_ETIQUETTE_ENTREEID, TETQ_ETIQUETTEID, TETQ_CODE, DEMANDEID, TETQ_DATE_CREATION, TETQ_DESC, TETQ_LIB, TDOM_LIB, TPRCP_LIB, IMAGE_URL)
                AS
                (
	                SELECT 0 as LEVEL, NULL as TETQ_ETIQUETTE_ENTREEID, TETQ.TETQ_ETIQUETTEID, TETQ.TETQ_CODE, TETQ.DEMANDEID, TETQ.TETQ_DATE_CREATION, TETQ.TETQ_DESC, TETQ.TETQ_LIB, TDOM.TDOM_LIB, TPRCP.TPRCP_LIB, 'root' as IMAGE_URL
		                FROM TETQ_ETIQUETTES as TETQ 
		                JOIN TOBJE_OBJET_ETIQUETTES TOBJE on TETQ.TOBJE_OBJET_ETIQUETTEID = TOBJE.TOBJE_OBJET_ETIQUETTEID 
		                JOIN TDOM_DOMAINES TDOM on TOBJE.TDOM_DOMAINEID  = TDOM.TDOM_DOMAINEID 
		                JOIN TPRCP_PRC_PERIMETRES TPRCP on TETQ.TPRCP_PRC_PERIMETREID  = TPRCP.TPRCP_PRC_PERIMETREID 
		            WHERE TETQ.TETQ_ETIQUETTEID = {etiquetteId}
	                UNION ALL
	                SELECT LEVEL+1 as LEVEL, parent.TETQ_ETIQUETTE_ENTREEID, parent.TETQ_ETIQUETTEID, TETQ.TETQ_CODE, TETQ.DEMANDEID, TETQ.TETQ_DATE_CREATION, TETQ.TETQ_DESC, TETQ.TETQ_LIB, TDOM.TDOM_LIB, TPRCP.TPRCP_LIB, 'leaf' as IMAGE_URL
		                FROM TSR_SUIVI_RESSOURCES as parent
		                JOIN TETQ_ETIQUETTES TETQ on parent.TETQ_ETIQUETTEID = TETQ.TETQ_ETIQUETTEID 
		                JOIN TOBJE_OBJET_ETIQUETTES TOBJE on TETQ.TOBJE_OBJET_ETIQUETTEID = TOBJE.TOBJE_OBJET_ETIQUETTEID 
		                JOIN TDOM_DOMAINES TDOM on TOBJE.TDOM_DOMAINEID = TDOM.TDOM_DOMAINEID 
		                JOIN TPRCP_PRC_PERIMETRES TPRCP on TETQ.TPRCP_PRC_PERIMETREID = TPRCP.TPRCP_PRC_PERIMETREID 
		                JOIN CONSOMMATION subqry on parent.TETQ_ETIQUETTE_ENTREEID = subqry.TETQ_ETIQUETTEID
                )
                SELECT LEVEL, TETQ_ETIQUETTEID as ID, NULLIF(TETQ_ETIQUETTE_ENTREEID, '') as PARENTID, TETQ_CODE, NULLIF(DEMANDEID, '') as DEMANDEID, TETQ_DATE_CREATION,
                       NULLIF(TETQ_DESC, '') as TETQ_DESC, NULLIF(TETQ_LIB, '') as TETQ_LIB, NULLIF(TDOM_LIB, '') as TDOM_LIB, NULLIF(TPRCP_LIB, '') as TPRCP_LIB, IMAGE_URL
                FROM CONSOMMATION
	                WHERE LEVEL < {level}";

    /// <summary>
    /// PRODUCTION : liste des etiquettes qui ont contribué a générer cette etiquette
    /// Présenter :du bas vers le haut ou de droite vers la gauche
    /// </summary>
    /// <param name="etiquetteId"></param>
    /// <returns></returns>
    public static string Production(int etiquetteId, int level = 5)
        => @$"SELECT LEVEL, ID, PARENTID, TETQ_CODE, DEMANDEID, TETQ_DATE_CREATION, TETQ_DESC, TETQ_LIB, TDOM_LIB, TPRCP_LIB, IMAGE_URL
                 FROM (
	                WITH RECURSIVE PRODUCTION (LEVEL, TETQ_ETIQUETTE_ENTREEID, TETQ_ETIQUETTEID, TETQ_CODE, DEMANDEID, TETQ_DATE_CREATION, TETQ_DESC, TETQ_LIB, TDOM_LIB, TPRCP_LIB, IMAGE_URL)
	                AS
	                (
	                SELECT 0 as LEVEL, TETQ.TETQ_ETIQUETTEID, NULL as TETQ_ETIQUETTE_ENTREEID, TETQ.TETQ_CODE, TETQ.DEMANDEID, TETQ.TETQ_DATE_CREATION, TETQ.TETQ_DESC, TETQ.TETQ_LIB, TDOM.TDOM_LIB, TPRCP.TPRCP_LIB, 'root' as IMAGE_URL
		                FROM  TETQ_ETIQUETTES as TETQ 
		                JOIN TOBJE_OBJET_ETIQUETTES TOBJE on TETQ.TOBJE_OBJET_ETIQUETTEID = TOBJE.TOBJE_OBJET_ETIQUETTEID 
		                JOIN TDOM_DOMAINES TDOM on TOBJE.TDOM_DOMAINEID  = TDOM.TDOM_DOMAINEID 
		                JOIN TPRCP_PRC_PERIMETRES TPRCP on TETQ.TPRCP_PRC_PERIMETREID = TPRCP.TPRCP_PRC_PERIMETREID 
	                WHERE TETQ.TETQ_ETIQUETTEID = {etiquetteId}
	                UNION ALL
	                SELECT LEVEL-1 as LEVEL, parent.TETQ_ETIQUETTE_ENTREEID, parent.TETQ_ETIQUETTEID, TETQ.TETQ_CODE, TETQ.DEMANDEID, TETQ.TETQ_DATE_CREATION, TETQ.TETQ_DESC, TETQ.TETQ_LIB, TDOM.TDOM_LIB, TPRCP.TPRCP_LIB, 'leaf' as IMAGE_URL
		                FROM TSR_SUIVI_RESSOURCES as parent
		                LEFT JOIN TETQ_ETIQUETTES TETQ on parent.TETQ_ETIQUETTE_ENTREEID =  TETQ.TETQ_ETIQUETTEID 
		                JOIN TOBJE_OBJET_ETIQUETTES TOBJE on TETQ.TOBJE_OBJET_ETIQUETTEID = TOBJE.TOBJE_OBJET_ETIQUETTEID 
		                JOIN TDOM_DOMAINES TDOM on TOBJE.TDOM_DOMAINEID  = TDOM.TDOM_DOMAINEID 
		                JOIN TPRCP_PRC_PERIMETRES TPRCP on TETQ.TPRCP_PRC_PERIMETREID  = TPRCP.TPRCP_PRC_PERIMETREID 
		                JOIN PRODUCTION subqry on parent.TETQ_ETIQUETTEID = subqry.TETQ_ETIQUETTE_ENTREEID
	                )
	                SELECT LEVEL, TETQ_ETIQUETTE_ENTREEID as ID, NULLIF(TETQ_ETIQUETTEID, '') as PARENTID, TETQ_CODE, NULLIF(DEMANDEID, '') as DEMANDEID, TETQ_DATE_CREATION, 
	                	   NULLIF(TETQ_DESC, '') as TETQ_DESC, NULLIF(TETQ_LIB, '') as TETQ_LIB, NULLIF(TDOM_LIB, '') as TDOM_LIB, NULLIF(TPRCP_LIB, '') as TPRCP_LIB, IMAGE_URL
	                FROM PRODUCTION
		                WHERE LEVEL > -{level}
		                AND TETQ_ETIQUETTE_ENTREEID IS NOT NULL  
                ) ORDER BY LEVEL DESC";

    /// <summary>
    /// RESSOURCES : liste des etiquettes qui ont contribué a générer cette etiquette
    /// </summary>
    /// <param name="labelCode">ex: '2022_PRS_TRP_07'</param>
    /// <returns></returns>
    public static string Ressources(string labelCode)
        => @$"SELECT EtqSortie AS LabelOutput, TSR_ENTREE AS Input, TETQ_CODE AS LabelInput, LEVEL AS LabelLevel, TOBJE_CODE AS LabelObjectCode
            FROM (
                WITH RECURSIVE PRODUCTION (LEVEL, TETQ_ETIQUETTE_ENTREEID, TETQ_ETIQUETTEID, EtqSortie, TETQ_CODE, TSR_ENTREE, DEMANDEID, TETQ_DATE_CREATION, TETQ_DESC, TETQ_LIB, TDOM_LIB, TPRCP_LIB, TOBJE_CODE, NATURE)
                AS
                (               
                    SELECT 0 AS LEVEL, TETQ.TETQ_ETIQUETTEID, NULL AS TETQ_ETIQUETTE_ENTREEID, NULL AS EtqSortie, TETQ.TETQ_CODE, NULL AS TSR_ENTREE, TETQ.DEMANDEID, TETQ.TETQ_DATE_CREATION, TETQ.TETQ_DESC, TETQ.TETQ_LIB, TDOM.TDOM_LIB, TPRCP.TPRCP_LIB, TOBJE.TOBJE_CODE, 'root' AS NATURE
                          FROM  TETQ_ETIQUETTES AS TETQ 
                          JOIN TOBJE_OBJET_ETIQUETTES TOBJE on TETQ.TOBJE_OBJET_ETIQUETTEID = TOBJE.TOBJE_OBJET_ETIQUETTEID 
                          JOIN TDOM_DOMAINES TDOM on TOBJE.TDOM_DOMAINEID  = TDOM.TDOM_DOMAINEID 
                          JOIN TPRCP_PRC_PERIMETRES TPRCP on TETQ.TPRCP_PRC_PERIMETREID = TPRCP.TPRCP_PRC_PERIMETREID 
                    WHERE TETQ.TETQ_CODE = '{labelCode}'
                    UNION ALL
                    SELECT LEVEL+1 AS LEVEL, parent.TETQ_ETIQUETTE_ENTREEID, parent.TETQ_ETIQUETTEID, subqry.TETQ_CODE as EtqSortie, TETQ.TETQ_CODE, parent.TSR_ENTREE , TETQ.DEMANDEID, TETQ.TETQ_DATE_CREATION, TETQ.TETQ_DESC, TETQ.TETQ_LIB, TDOM.TDOM_LIB, TPRCP.TPRCP_LIB, TOBJE.TOBJE_CODE, 'leaf' AS NATURE
                          FROM TSR_SUIVI_RESSOURCES AS parent
                          LEFT JOIN TETQ_ETIQUETTES TETQ on parent.TETQ_ETIQUETTE_ENTREEID =  TETQ.TETQ_ETIQUETTEID 
                          JOIN TOBJE_OBJET_ETIQUETTES TOBJE on TETQ.TOBJE_OBJET_ETIQUETTEID = TOBJE.TOBJE_OBJET_ETIQUETTEID 
                          JOIN TDOM_DOMAINES TDOM on TOBJE.TDOM_DOMAINEID  = TDOM.TDOM_DOMAINEID 
                          JOIN TPRCP_PRC_PERIMETRES TPRCP on TETQ.TPRCP_PRC_PERIMETREID  = TPRCP.TPRCP_PRC_PERIMETREID 
                          JOIN PRODUCTION subqry on parent.TETQ_ETIQUETTEID = subqry.TETQ_ETIQUETTE_ENTREEID    
                )
                SELECT LEVEL, TETQ_ETIQUETTE_ENTREEID, NULLIF(TETQ_ETIQUETTEID, '') AS PARENTID,  NULLIF(EtqSortie, '') AS EtqSortie, TETQ_CODE, NULLIF(TSR_ENTREE, '') AS TSR_ENTREE, NULLIF(DEMANDEID, '') AS DEMANDEID, TETQ_DATE_CREATION, NULLIF(TETQ_DESC, '') AS TETQ_DESC, NULLIF(TETQ_LIB, '') AS TETQ_LIB, NULLIF(TDOM_LIB, '') AS TDOM_LIB, NULLIF(TPRCP_LIB, '') AS TPRCP_LIB, TOBJE_CODE, NATURE
                FROM PRODUCTION
                    WHERE LEVEL != 0 
                    AND TETQ_ETIQUETTE_ENTREEID IS NOT NULL
            ) ORDER BY LEVEL DESC";
}

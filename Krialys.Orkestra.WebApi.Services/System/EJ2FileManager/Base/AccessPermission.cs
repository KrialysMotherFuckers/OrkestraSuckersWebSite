﻿namespace Krialys.Orkestra.WebApi.Services.EJ2FileManager.Base;

public class AccessPermission
{
    public bool Copy = true;
    public bool Download = true;
    public bool Write = true;
    public bool WriteContents = true;
    public bool Read = true;
    public bool Upload = true;
    public string Message = String.Empty;
}
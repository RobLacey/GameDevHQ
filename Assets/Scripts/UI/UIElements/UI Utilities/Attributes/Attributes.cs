using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class DefectTrackAttribute : Attribute
{
    private readonly string _defectID ;
    private DateTime _modificationDate ;
    private readonly string _developerID ;
    private Origin _defectOrigin ;
    private string _fixComment ;
    private float _version;

    public DefectTrackAttribute( string lcDefectID, string lcModificationDate, string lcDeveloperID )
    { 
        _defectID = lcDefectID ;
        _modificationDate = DateTime.Parse(lcModificationDate);
        _developerID = lcDeveloperID ; 
    }

    public string DefectID => _defectID;

    public string ModificationDate
    {
        get => _modificationDate.ToShortDateString();
    }

    public string DeveloperID => _developerID;

    public Origin Origin
    { 
        get => _defectOrigin ; 
        set => _defectOrigin = value;
    }

    public string FixComment
    { 
        get => _fixComment;
        set => _fixComment = value;
    }
    
    public float Version
    { 
        get => _version;
        set => _version = value;
    }
}

public enum Origin { Testing, Playing, User, QA }



[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ClassInjectAttribute : Attribute
{
    public ClassInjectAttribute() { }
}      

 

[AttributeUsage(AttributeTargets.Property)]
public class ServiceInjectAttribute : Attribute
{
    public ServiceInjectAttribute() { }
}


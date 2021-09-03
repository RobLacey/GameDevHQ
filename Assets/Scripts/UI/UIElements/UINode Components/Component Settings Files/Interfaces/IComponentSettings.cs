using EZ.Inject;

public interface IComponentSettings: IParameters
{
    NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions);
}
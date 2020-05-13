public interface ISettings
{
    bool IsActive { get; set; }
    void SetActive(bool active);
}

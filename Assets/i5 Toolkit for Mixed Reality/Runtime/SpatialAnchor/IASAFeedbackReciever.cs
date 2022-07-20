public interface IASAFeedbackReciever
{
    void CreateAnchorStarted();
    void CreateAnchorFinalized();
    void CreateAnchorStatusUpdate(ASAStaus status);
}

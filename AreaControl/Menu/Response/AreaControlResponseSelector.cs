namespace AreaControl.Menu.Response
{
    public class AreaControlResponseSelector : AbstractResponseSelector
    {
        public AreaControlResponseSelector(IResponseManager responseManager) 
            : base(responseManager)
        {
        }

        /// <inheritdoc />
        public override MenuType Type => MenuType.AREA_CONTROL;
    }
}
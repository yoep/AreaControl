namespace AreaControl.Menu.Response
{
    public class StreetControlResponseSelector : AbstractResponseSelector
    {
        public StreetControlResponseSelector(IResponseManager responseManager)
            : base(responseManager)
        {
        }

        /// <inheritdoc />
        public override MenuType Type => MenuType.STREET_CONTROL;
    }
}
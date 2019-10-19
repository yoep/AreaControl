using AreaControl.Instances;

namespace AreaControl.Duties.Exceptions
{
    public class DutyGroupMappingException : DutyException
    {
        public DutyGroupMappingException(PedType pedType)
            : base($"Unknown duty group mapping for ped type {pedType}")
        {
            PedType = pedType;
        }

        /// <summary>
        /// Get the ped type that caused the exception.
        /// </summary>
        public PedType PedType { get; }
    }
}
using System;
using System.Runtime.InteropServices;
using Nefarius.ViGEm.Client.Exceptions;

namespace Nefarius.ViGEm.Client.Targets
{
    /// <inheritdoc />
    /// <summary>
    ///     Represents an emulated wired Sony DualShock 4 Controller.
    /// </summary>
    public class NintendoSwitchController : ViGEmTarget
    {
        private ViGEmClient.PVIGEM_NSWITCH_NOTIFICATION _notificationCallback;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Nefarius.ViGEm.Client.Targets.DualShock4Controller" /> class bound
        ///     to a <see cref="T:Nefarius.ViGEm.Client.ViGEmClient" />.
        /// </summary>
        /// <param name="client">The <see cref="T:Nefarius.ViGEm.Client.ViGEmClient" /> this device is attached to.</param>
        public NintendoSwitchController(ViGEmClient client) : base(client)
        {
            NativeHandle = ViGEmClient.vigem_target_nswitch_alloc();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Nefarius.ViGEm.Client.Targets.DualShock4Controller" /> class bound
        ///     to a <see cref="T:Nefarius.ViGEm.Client.ViGEmClient" /> overriding the default Vendor and Product IDs with the
        ///     provided values.
        /// </summary>
        /// <param name="client">The <see cref="T:Nefarius.ViGEm.Client.ViGEmClient" /> this device is attached to.</param>
        /// <param name="vendorId">The Vendor ID to use.</param>
        /// <param name="productId">The Product ID to use.</param>
        public NintendoSwitchController(ViGEmClient client, ushort vendorId, ushort productId) : this(client)
        {
            VendorId = vendorId;
            ProductId = productId;
        }

        /// <summary>
        ///     Submits a report to this device which will update its state.
        /// </summary>
        /// <param name="report">report buffer to submit</param>
        /// <param name="timerStatus">timer status for report (0 - disable, 1 - enable and save report, 2 - ignore)</param>
        public void SendReport(byte[] report, byte timerStatus = 2)
        {
            var error = ViGEmClient.vigem_target_nswitch_update(Client.NativeHandle, NativeHandle, report, timerStatus);

            switch (error)
            {
                case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_BUS_NOT_FOUND:
                    throw new VigemBusNotFoundException();
                case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_INVALID_TARGET:
                    throw new VigemInvalidTargetException();
            }
        }

        public override void Connect()
        {
            base.Connect();

            //
            // Callback to event
            // 
            _notificationCallback = (client, target, report) =>
            {
                FeedbackReceived?.Invoke(this, report);
            };

            var error = ViGEmClient.vigem_target_nswitch_register_notification(Client.NativeHandle, NativeHandle,
                _notificationCallback);

            switch (error)
            {
                case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_BUS_NOT_FOUND:
                    throw new VigemBusNotFoundException();
                case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_INVALID_TARGET:
                    throw new VigemInvalidTargetException();
                case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_CALLBACK_ALREADY_REGISTERED:
                    throw new VigemCallbackAlreadyRegisteredException();
            }
        }

        public override void Disconnect()
        {
            ViGEmClient.vigem_target_nswitch_unregister_notification(NativeHandle);

            base.Disconnect();
        }

        public event Action<object, byte[]> FeedbackReceived;
    }
}
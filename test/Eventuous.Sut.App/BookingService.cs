using Eventuous.Sut.Domain;

namespace Eventuous.Sut.App;

using static Commands;

public class BookingService : CommandService<Booking, BookingState, BookingId> {
    public BookingService(IEventStore eventStore, StreamNameMap? streamNameMap = null) : this(eventStore, eventStore, streamNameMap) { }

    public BookingService(IEventReader reader, IEventWriter writer, StreamNameMap? streamNameMap = null) : base(reader, writer, streamNameMap: streamNameMap) {
        On<BookRoom>()
            .InState(ExpectedState.New)
            .GetId(cmd => new(cmd.BookingId))
            .ActAsync(
                (booking, cmd, _) => {
                    booking.BookRoom(cmd.RoomId, new(cmd.CheckIn, cmd.CheckOut), new(cmd.Price));

                    return Task.CompletedTask;
                }
            );

        On<ImportBooking>()
            .InState(ExpectedState.New)
            .GetId(cmd => new(cmd.BookingId))
            .Act((booking, cmd) => booking.Import(cmd.RoomId, new(cmd.CheckIn, cmd.CheckOut), new(cmd.Price)));

        On<RecordPayment>()
            .InState(ExpectedState.Existing)
            .GetId(cmd => cmd.BookingId)
            .Act((booking, cmd) => booking.RecordPayment(cmd.PaymentId, cmd.Amount, cmd.PaidAt));
    }
}

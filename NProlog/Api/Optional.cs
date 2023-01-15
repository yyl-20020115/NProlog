namespace Org.NProlog.Api;

public readonly struct Optional<T>
{
    public static readonly Optional<T> EMPTY = new();
    public static Optional<T> Empty() => new();
    public static Optional<T> Of(T value) => new(value);
    private readonly T? value;
    public bool HasValue => this.value != null;
    public bool IsOptional => !this.HasValue;
    public T? Value => value;
    public Optional(T? value = default) => this.value = value;
}

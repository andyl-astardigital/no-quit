using NoQuit.Core.Model;

namespace NoQuit.Core.Domain;

public readonly record struct PixelInstruction(int X, int Y, int Size, SpriteCell Cell);

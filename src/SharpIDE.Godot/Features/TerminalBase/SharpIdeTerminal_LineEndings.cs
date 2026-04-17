using System.Buffers;

namespace SharpIDE.Godot.Features.TerminalBase;

public partial class SharpIdeTerminal
{
    private bool _previousArrayEndedInCr = false;

    // Unfortunately, although the terminal emulator handles escape sequences etc, it does not handle interpreting \n as \r\n - that is handled by the PTY, which we currently don't use
    // So we need to replace lone \n with \r\n ourselves
    // TODO: Probably run processes with PTY instead, so that this is not needed, and so we can capture user input and Ctrl+C etc
    // 🤖
    // processed should be a ReadOnlySpan, but Godot currently can't cast a ReadOnlySpan<T> to Variant, only Span<T> 
    private void ProcessLineEndings(ReadOnlySpan<byte> input, Span<byte> workingBuffer, out Span<byte> processed)
    {
        if (input.Length == 0)
        {
            processed = Span<byte>.Empty;
            return;
        }
        // Count how many \n need to be replaced (those not preceded by \r)
        var replacementCount = 0;
        var previousWasCr = _previousArrayEndedInCr;

        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] == (byte)'\n')
            {
                // Check if it's preceded by \r
                var precededByCr = (i > 0 && input[i - 1] == (byte)'\r') || (i == 0 && previousWasCr);
                if (!precededByCr)
                {
                    replacementCount++;
                }
            }

            previousWasCr = input[i] == (byte)'\r';
        }

        // If no replacements needed, return original array
        if (replacementCount == 0)
        {
            input.CopyTo(workingBuffer);
            processed = workingBuffer[..input.Length];
            return;
        }

        var writeIndex = 0;
        previousWasCr = _previousArrayEndedInCr;

        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] == (byte)'\n')
            {
                // Check if it's preceded by \r
                var precededByCr = (i > 0 && input[i - 1] == (byte)'\r') || (i == 0 && previousWasCr);
                if (!precededByCr)
                {
                    workingBuffer[writeIndex++] = (byte)'\r';
                }
            }

            workingBuffer[writeIndex++] = input[i];
            previousWasCr = input[i] == (byte)'\r';
        }

        processed = workingBuffer[..writeIndex];
    }
}
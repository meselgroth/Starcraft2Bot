# Processing Bitmaps
The SC2 Api returns bitmaps of what the map looks like. To understand how to process this, I looked at an existing example [SC2-CSharpe-Starterkit](https://github.com/NikEyX/SC2-CSharpe-Starterkit). Essentially the critical method this one with a confusing looking bitwise operation.

```csharp
public static int GetDataValueBit(ImageData data, int x, int y)
{
    int pixelID = x + y * data.Size.X;
    int byteLocation = pixelID / 8;
    int bitLocation = pixelID % 8;
    int bit = data.Data[byteLocation] & (1 << (7 - bitLocation));
    return bit == 0 ? 0 : 1;
}
```
At first I just used this at face value: it returns 0 if a position(x,y) is white, 1 if black.
Then I decided to properly relearn bitwise operators (I started vaguely remembering, I had learned this in uni, It has to do with electronic circuits and the logic gates that can turn a following circuit on or off (bit output) after receiving on or off electric signal (bit input).)

There's plenty of info out there on what each operator does, I will just step through each line.

Let's start with regular dotnet types and recreate each line, in a format that will work in a unit test or Linqpad.

Imagine our Bitmap, each pixel is a bit:
>`0 0 0 0 0` 5 pixels width  
`0 0 0 2 0`  times 2 pixels height  

Stored in bytes
>` 0000 0000, 2000 0000` Note: we have 6 extra bits overhead  
Using a 0 based position of (x,y), our colour `2` is the 8th bit.

```csharp
// First byte is 0
var data = new byte[]{0,0};
// For 2nd byte, create new BitArray to represent 1 0 0 0 0 0 0 0
var bitArr = new System.Collections.BitArray(new bool[]{false,false,false,false,false,false,false,true});
// OR
Convert.ToByte("10000000", 2);

// Convert to Byte and place in second position in data
bitArr.CopyTo(data, 1); // data: [0]: 0, [1]: 128

var b = 1<<7; // 128
var bitcompare = data[1] & b; // returns 128. Although a byte is returned, Bitwise AND compares a single bit. So it is comparing the first bit of each byte and as they match it returns the number representation of the full byte of that single bit. 1 1 1 1 1 1 1 1 & 1 0 0 0 0 0 0 0 also returns 128
```

Let's return to our original method.  
Assuming the bitmap of pixels in our ImageData.Data:
>`0 0 0 0 0`  
`0 0 0 2 0`

When we call it for a position of `x=3`, `y=1`
```csharp
public static int GetDataValueBit(ImageData data, int x, int y)
{
    int pixelID = x + y * data.Size.X;  // 8 pixels
    int byteLocation = pixelID / 8;  // 1 (2nd byte): 1bit per pixel, 8bits per byte
    int bitLocation = pixelID % 8;  // 0 (1st bit)
    int bit = data.Data[byteLocation] & (1 << (7 - bitLocation)); // Test binary rep of byte(128=1 0 0 0 0 0 0 0) is same as binary rep of (1 left shift 7 bits to get to first bit 1 0 0 0 0 0 0 0, which = 1)
    return bit == 0 ? 0 : 1;
}
```
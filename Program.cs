// See https://aka.ms/new-console-template for more information
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using WaveFunctionCollapseTest;

Console.WriteLine("Hello, World!");

var sampleText = @"
MMMMMPPPPMMP
MMMMPPPPMMPP
MMPMPPPPMPPP
PMPPPPSPPPPP
PPPPPSWSPPPP
PPSSSWWWSSPP
SSSWWWWWWSSP";

// Create a unique list of tile types
List<char> tileTypes = [];
foreach (char c in sampleText)
{
    if (!char.IsWhiteSpace(c) && !tileTypes.Contains(c))
    {
        tileTypes.Add(c);
    }
}

// Convert the sampleText into a 2D array
string[] lines = sampleText.Trim().Split(Environment.NewLine);
int rows = lines.Length;
int cols = lines[0].Length;
char[,] sample = new char[rows, cols];

for (int y = 0; y < rows; y++)
{
    for (int x = 0; x < cols; x++)
    {
        sample[y, x] = lines[y][x];
    }
}

var probabilities = new List<Probability>();

//Initialize the probabilities list with all tile types
foreach (var type in tileTypes)
{
    probabilities.Add(new Probability(type, tileTypes));
}

// Set the probabilities for each tile type based on the sample
for (int y = 0; y < sample.GetLength(0); y++)
{
    for (int x = 0; x < sample.GetLength(1); x++)
    {
        char currentType = sample[y, x];
        var currentProbability = probabilities.First(p => p.Type == currentType);
        // Check top neighbor
        if (y > 0)
        {
            char topType = sample[y - 1, x];
            currentProbability.Top[topType]++;
        }
        // Check right neighbor
        if (x < sample.GetLength(1) - 1)
        {
            char rightType = sample[y, x + 1];
            currentProbability.Right[rightType]++;
        }
        // Check bottom neighbor
        if (y < sample.GetLength(0) - 1)
        {
            char bottomType = sample[y + 1, x];
            currentProbability.Bottom[bottomType]++;
        }
        // Check left neighbor
        if (x > 0)
        {
            char leftType = sample[y, x - 1];
            currentProbability.Left[leftType]++;
        }
    }
}

// Print the probabilities
foreach (Probability probability in probabilities)
{
    Console.WriteLine($"Type: {probability.Type}");
    Console.WriteLine($"  Top: {string.Join(", ", probability.Top.Select(kv => $"{kv.Key}:{kv.Value}"))}");
    Console.WriteLine($"  Right: {string.Join(", ", probability.Right.Select(kv => $"{kv.Key}:{kv.Value}"))}");
    Console.WriteLine($"  Bottom: {string.Join(", ", probability.Bottom.Select(kv => $"{kv.Key}:{kv.Value}"))}");
    Console.WriteLine($"  Left: {string.Join(", ", probability.Left.Select(kv => $"{kv.Key}:{kv.Value}"))}");
}

// Create a game array and populate it with random tiles based on the probabilities
int gameWidth = 10;
int gameHeight = 10;
var totalTiles = gameWidth * gameHeight;
var settledTiles = 0;

char[,] game = new char[gameHeight, gameWidth];
Probability[,] gameProbabilities = new Probability[gameHeight, gameWidth];
Random rand = new Random();

// Initialize game array with null characters to indicate unsettled tiles
for (int y = 0; y < gameHeight; y++)
{
    for (int x = 0; x < gameWidth; x++)
    {
        game[y, x] = '\0'; // Null character indicates unsettled
    }
}

// Loop until all tiles are settled
do
{
    // Calculate the probabilities for every cell in the game array in gameProbabilities
    for (int y = 0; y < gameHeight; y++)
    {
        for (int x = 0; x < gameWidth; x++)
        {
            if (game[y, x] == '\0') // Only calculate for unsettled tiles
            {
                gameProbabilities[y, x] = new Probability('\0', tileTypes);
                
                // Calculate valid tile types based on neighbors
                foreach (var tileType in tileTypes)
                {
                    var probability = probabilities.First(p => p.Type == tileType);
                    int totalCompatibility = 0;

                    // Check compatibility with top neighbor
                    if (y > 0)
                    {
                        char topNeighbor = game[y - 1, x];
                        if (topNeighbor != '\0')
                        {
                            totalCompatibility += probability.Top[topNeighbor];
                        }
                        else
                        {
                            totalCompatibility += probability.Top.Values.Sum();
                        }
                    }

                    // Check compatibility with right neighbor
                    if (x < gameWidth - 1)
                    {
                        char rightNeighbor = game[y, x + 1];
                        if (rightNeighbor != '\0')
                        {
                            totalCompatibility += probability.Right[rightNeighbor];
                        }
                        else
                        {
                            totalCompatibility += probability.Right.Values.Sum();
                        }
                    }

                    // Check compatibility with bottom neighbor
                    if (y < gameHeight - 1)
                    {
                        char bottomNeighbor = game[y + 1, x];
                        if (bottomNeighbor != '\0')
                        {
                            totalCompatibility += probability.Bottom[bottomNeighbor];
                        }
                        else
                        {
                            totalCompatibility += probability.Bottom.Values.Sum();
                        }
                    }

                    // Check compatibility with left neighbor
                    if (x > 0)
                    {
                        char leftNeighbor = game[y, x - 1];
                        if (leftNeighbor != '\0')
                        {
                            totalCompatibility += probability.Left[leftNeighbor];
                        }
                        else
                        {
                            totalCompatibility += probability.Left.Values.Sum();
                        }
                    }

                    // Store the compatibility score for this tile type at this position
                    gameProbabilities[y, x].Top[tileType] = totalCompatibility;
                }
            }
        }
    }

    // Find the cell with the lowest entropy (most constrained)
    int minEntropy = int.MaxValue;
    int bestY = -1, bestX = -1;

    for (int y = 0; y < gameHeight; y++)
    {
        for (int x = 0; x < gameWidth; x++)
        {
            if (game[y, x] == '\0') // Only consider unsettled tiles
            {
                var validOptions = gameProbabilities[y, x].Top.Where(kv => kv.Value > 0).Count();
                if (validOptions > 0 && validOptions < minEntropy)
                {
                    minEntropy = validOptions;
                    bestY = y;
                    bestX = x;
                }
            }
        }
    }

    // Settle that cell to a specific tile type based on the calculated probabilities
    if (bestY != -1 && bestX != -1)
    {
        var cellProbabilities = gameProbabilities[bestY, bestX].Top
            .Where(kv => kv.Value > 0)
            .ToList();

        if (cellProbabilities.Any())
        {
            // Weighted random selection based on compatibility scores
            int totalWeight = cellProbabilities.Sum(kv => kv.Value);
            int randomValue = rand.Next(totalWeight);
            int currentWeight = 0;

            foreach (var kvp in cellProbabilities)
            {
                currentWeight += kvp.Value;
                if (randomValue < currentWeight)
                {
                    game[bestY, bestX] = kvp.Key;
                    settledTiles++;
                    break;
                }
            }
        }
        else
        {
            // Fallback: choose a random tile type if no valid options
            game[bestY, bestX] = tileTypes[rand.Next(tileTypes.Count)];
            settledTiles++;
        }
    }
    else
    {
        // If no valid cell found, settle remaining unsettled tiles randomly
        for (int y = 0; y < gameHeight; y++)
        {
            for (int x = 0; x < gameWidth; x++)
            {
                if (game[y, x] == '\0')
                {
                    game[y, x] = tileTypes[rand.Next(tileTypes.Count)];
                    settledTiles++;
                }
            }
        }
    }

} while (settledTiles < totalTiles);

// Print the generated game grid
Console.WriteLine("\nGenerated Game Grid:");
for (int y = 0; y < gameHeight; y++)
{
    for (int x = 0; x < gameWidth; x++)
    {
        Console.Write(game[y, x]);
    }
    Console.WriteLine();
}

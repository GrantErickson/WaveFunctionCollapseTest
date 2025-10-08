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
SSSSSWWWSSSS
WWWWWWWWWWWW";

sampleText = @"
MMMMMPPPPMMM
MMMPPSPPMMMM
MMPSSWSPMMMM
MMPPSWWSPMMM
MMPSWWWSPPPM
MMPSSSSSSPMM
MMMPPPPPPPMM
MMMMMMMMMMMM";

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
int gameWidth = 20;
int gameHeight = 10;
var totalTiles = gameWidth * gameHeight;
var settledTiles = 0;

char[,] game = new char[gameHeight, gameWidth];
// This will hold the calculated probabilities for each cell in the game array
// Create a two dimensional array of Dictionary<char, int> called gameProbabilities 
Dictionary<char, int>[,] gameProbabilities = new Dictionary<char, int>[gameHeight, gameWidth];
Random rand = new Random();

// Initialize game array with null characters to indicate unsettled tiles
for (int y = 0; y < gameHeight; y++)
{
    for (int x = 0; x < gameWidth; x++)
    {
        game[y, x] = '\0'; // Null character indicates unsettled
        gameProbabilities[y, x] = new Dictionary<char, int>();
        foreach (var type in tileTypes)
        {
            gameProbabilities[y, x][type] = 0; // Initialize all probabilities to 0
        }
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
                // Calculate the cell's probabilities based on its neighbors
                var cellProbabilities = new Dictionary<char, int>();
                foreach (var type in tileTypes)
                {
                    cellProbabilities[type] = 0; // Initialize all probabilities to 0
                }
                // Check top neighbor
                if (y > 0 && game[y - 1, x] != '\0')
                {
                    char topType = game[y - 1, x];
                    var topProb = probabilities.First(p => p.Type == topType);
                    foreach (var kv in topProb.Bottom)
                    {
                        if (kv.Value == 0) cellProbabilities[kv.Key] = int.MinValue;
                        else cellProbabilities[kv.Key] += kv.Value;
                    }
                }
                // Check right neighbor
                if (x < gameWidth - 1 && game[y, x + 1] != '\0')
                {
                    char rightType = game[y, x + 1];
                    var rightProb = probabilities.First(p => p.Type == rightType);
                    foreach (var kv in rightProb.Left)
                    {
                        if (kv.Value == 0) cellProbabilities[kv.Key] = int.MinValue;
                        else cellProbabilities[kv.Key] += kv.Value;
                    }
                }
                // Check bottom neighbor
                if (y < gameHeight - 1 && game[y + 1, x] != '\0')
                {
                    char bottomType = game[y + 1, x];
                    var bottomProb = probabilities.First(p => p.Type == bottomType);
                    foreach (var kv in bottomProb.Top)
                    {
                        if (kv.Value == 0) cellProbabilities[kv.Key] = int.MinValue;
                        else cellProbabilities[kv.Key] += kv.Value;
                    }
                }
                // Check left neighbor
                if (x > 0 && game[y, x - 1] != '\0')
                {
                    char leftType = game[y, x - 1];
                    var leftProb = probabilities.First(p => p.Type == leftType);
                    foreach (var kv in leftProb.Right)
                    {
                        if (kv.Value == 0) cellProbabilities[kv.Key] = int.MinValue;
                        else cellProbabilities[kv.Key] += kv.Value;
                    }
                }
                // Set any negative probabilities to zero
                foreach (var type in tileTypes)
                {
                    if (cellProbabilities[type] < 0)
                    {
                        cellProbabilities[type] = 0;
                    }
                }
                gameProbabilities[y, x] = cellProbabilities;
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
                int entropy = gameProbabilities[y, x].Values.Sum();
                if (entropy == 0 && minEntropy == int.MaxValue )
                {
                    bestY = y;
                    bestX = x;
                    minEntropy = int.MaxValue - 1;
                }
                else if (entropy > 0 && entropy < minEntropy)
                {
                    minEntropy = entropy;
                    bestY = y;
                    bestX = x;
                }
            }
        }
    }

    // Settle that cell to a specific tile type based on the calculated probabilities
    if (bestY != -1 && bestX != -1)
    {
        var cellProbabilities = gameProbabilities[bestY, bestX];
        int totalProbability = cellProbabilities.Values.Sum();
        if (totalProbability > 0)
        {
            int randomValue = rand.Next(totalProbability);
            int cumulative = 0;
            foreach (var kv in cellProbabilities)
            {
                cumulative += kv.Value;
                if (randomValue < cumulative)
                {
                    if (kv.Key == 'W')
                    {
                        //Console.WriteLine("Here");
                    }
                    game[bestY, bestX] = kv.Key;
                    settledTiles++;
                    break;
                }
            }
        }
        else
        {
            // If all probabilities are zero, Set to most common value around it
            var neighborTypes = new List<char>();
            // Check top neighbor
            if (bestY > 0 && game[bestY - 1, bestX] != '\0')
            {
                neighborTypes.Add(game[bestY - 1, bestX]);
            }
            // Check right neighbor
            if (bestX < gameWidth - 1 && game[bestY, bestX + 1] != '\0')
            {
                neighborTypes.Add(game[bestY, bestX + 1]);
            }
            // Check bottom neighbor
            if (bestY < gameHeight - 1 && game[bestY + 1, bestX] != '\0')
            {
                neighborTypes.Add(game[bestY + 1, bestX]);
            }
            // Check left neighbor
            if (bestX > 0 && game[bestY, bestX - 1] != '\0')
            {
                neighborTypes.Add(game[bestY, bestX - 1]);
            }
            if (neighborTypes.Count > 0)
            {
                var mostCommon = neighborTypes
                    .GroupBy(t => t)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;
                game[bestY, bestX] = mostCommon;
            }
            else
            {
                // If no neighbors are settled, settle randomly
                game[bestY, bestX] = tileTypes[rand.Next(tileTypes.Count)];
            }
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
    PrintGame();
} while (settledTiles < totalTiles);

PrintGame();

void PrintGame()
{
    Console.Clear();
    // Print the generated game grid
    Console.WriteLine("\nGenerated Game Grid:");
    for (int y = 0; y < gameHeight; y++)
    {
        for (int x = 0; x < gameWidth; x++)
        {
            // Color the output: Make W White, P Green, S Brown, W Green
            switch (game[y, x])
            {
                case 'M':
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case 'P':
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case 'S':
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case 'W':
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            if (game[y,x] == '\0') Console.Write('.');
            else Console.Write(game[y, x]);


        }
        Console.WriteLine();
    }
}
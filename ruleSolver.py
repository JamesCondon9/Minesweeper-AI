import sweeperLib
from sweeperLib import MineNotWithinGridException
import time
from itertools import groupby, product

def step_solver(minefield):
    # Attempt to take a step??? Hope for the best maybe just click a random tile or something
    for y, row  in enumerate(minefield):
        for x, tile in enumerate(row):
            if(tile == "covered" or tile == "empty" or tile == "flagged"):
                continue # Skip non-number tiles.
            else:
                #print(f"\nStepping from row {y} column {x} tile...")
                basic_checks(minefield, x, y)


def basic_checks(minefield, x, y):
    
    # TEMPORARY TEST BLOCK ~~~~~~
    """ if(y != 8 or x != 28):
        return """

    # TEMPORARY TEST BLOCK ~~~~~~

    try:
        tile_value = int(minefield[y][x])
    except:
        raise Exception("Error - basic_checks function should only be called on tiles that have number values")
    
    
    try:
        flagged_count, covered_count, surrounding_tiles = find_surrounding_tiles(minefield, x, y)
    except TypeError: # If nothing is returned from find_surrounding_tiles
        return

    touching_count = flagged_count + covered_count
    surrounding_adjacent_covered_tiles = []
    surrounding_number_tiles = []

    #print(f"covered: {covered_count} flagged: {flagged_count} touching: {touching_count}")

    # Check if number tile is already solved, and if so, ignore it.
    if(tile_value == flagged_count and covered_count == 0):
        return

    # Basic Pattern [B1]
    
    if(tile_value-flagged_count == covered_count):
        #print("BASIC CHECK B1 ACTIVATED!")
        for (row, col), value in surrounding_tiles.items(): # Flag all covered tiles as they are all mines
           # print(f"Surrouning Tile: {(row, col)} Value: {value}")
            if(value == "covered"):
                #print(f"Flagging {(row, col)}")
                sweeperLib.flag_tile((col, row))
                minefield[row][col] = "flagged"
        return
    
    # Basic Pattern [B2]
    elif(tile_value == flagged_count):
        #print("BASIC CHECK B2 ACTIVATED!")
        for (row, col), value in surrounding_tiles.items(): # Click all covered tiles as there are no remaining mines
            if(value == "covered"):
                #print(f"Uncovering {(row, col)}")
                sweeperLib.uncover_tile((col, row))
        return

    
    else: # Prepare more data for next round of basic checks on the tile
        surrounding_adjacent_covered_tiles = find_adjacent_covered_tiles(surrounding_tiles)
        surrounding_number_tiles = find_surrounding_number_tiles(surrounding_tiles)
   
    
    # TEMP TESTING CODE BLOCK ~~~~~~~~~~   
    surrounding_adjacent_covered_tiles = find_adjacent_covered_tiles(surrounding_tiles)
    surrounding_number_tiles = find_surrounding_number_tiles(surrounding_tiles)     
    # TEMP TESTING CODE BLOCK ~~~~~~~~~~   

    # Basic Patterns [1-1], [1-1+], [1-2] and [1-2+]

    if(tile_value == 1 and only_has_one_N_length_sublist(surrounding_adjacent_covered_tiles, 2)):
        #print(f"RUNNING [1-1] / [1-1+] PATTERN on {(y,x)}")
        tile_index = 0 # Used to pick "the other one" out of the list of two coordinates, i.e. from the corner tile, used to find the index of the edge tile with a mod operation
        for tile in surrounding_adjacent_covered_tiles[0]:
            tile_index += 1
            # Only process the tile of the pair that is the corner tile (this will tell us where the second number tile is located)
            if(is_corner(tile, (y,x))):
                edge_tile = surrounding_adjacent_covered_tiles[0][tile_index % 2]

                delta_y = tile[0] - edge_tile[0]
                delta_x = tile[1] - edge_tile[1]

                new_tile_value = minefield[y+delta_y][x+delta_x]

                # [1-1] and [1-1+]
                if(new_tile_value == "1"):
                    _, _, new_surrounding_tiles = find_surrounding_tiles(minefield, x+delta_x, y+delta_y)

                    # Click all tiles that aren't the original two adjacent ones
                    for new_surrounding_tile in new_surrounding_tiles:
                        # Check it wasn't one of the originals and is a covered tile
                        if(not new_surrounding_tile in surrounding_tiles and minefield[new_surrounding_tile[0]][new_surrounding_tile[1]] == "covered"):
                            try:
                                #print(f"Uncovering {(new_surrounding_tile[0], new_surrounding_tile[1])} by [1-1] rule from {y,x}")
                                sweeperLib.uncover_tile((new_surrounding_tile[1], new_surrounding_tile[0]))
                            except MineNotWithinGridException as e:
                                print(f"Invalid mine coordinate: {e}")
                    
                    return


                # <<<<<<<<<<<<<<<<<<<<<< DEFINITELY SOMETHING WEIRD GOING ON HERE, IN THE SEED I LEFT THE PROGRAM IN, IT UNFLAGGED AROUND THE 6
                # [1-2] and [1-2+]
                elif(int(new_tile_value) >= 2 and int(new_tile_value) <= 8):
                    flagged_count, covered_count, new_surrounding_tiles = find_surrounding_tiles(minefield, x+delta_x, y+delta_y)

                    # Remaining tiles must be mines if there is exactly one mine in the original two adjacent tiles
                    if(covered_count-2 == int(new_tile_value)-flagged_count-1):
                        # Flag all remaining unflagged tiles that are not in the original two adjacent tiles
                        for new_surrounding_tile in new_surrounding_tiles:
                            # Check it wasn't one of the originals and is a covered tile
                            if(not new_surrounding_tile in surrounding_tiles and minefield[new_surrounding_tile[0]][new_surrounding_tile[1]] == "covered"):
                                try:
                                    print(f"\n\nflagged: {flagged_count} covered: {covered_count}")
                                    print(f"new surrounding tiles: {new_surrounding_tiles}")
                                    print(f"Flagging {(new_surrounding_tile[0], new_surrounding_tile[1])} by [1-{new_tile_value}] rule from {y,x}")
                                    sweeperLib.flag_tile((new_surrounding_tile[1], new_surrounding_tile[0]))
                                except MineNotWithinGridException as e:
                                    print(f"Invalid mine coordinate: {e}")



        return
   
                
def find_surrounding_tiles(minefield, x, y):
    flagged_count = 0
    covered_count = 0

    surrounding_tiles = {} # Key = Tile Coordinates (row, column), Value = Tile Value

    # Find each of the surrounding tiles
    for i in range(-1,2):
        for j in range(-1,2):
            if(i == 0 and j == 0):
                continue # Skip the tile itself

            surrounding_row = y+i
            surrounding_col = x+j

            if(surrounding_row < 0 or surrounding_col < 0 or surrounding_row > gridSize[0]-1 or surrounding_col > gridSize[1]-1): # Check surrounding mines are not on the edge of the board
                continue

            surrounding_tile = minefield[surrounding_row][surrounding_col]
            match(surrounding_tile):
                case "flagged":
                    flagged_count += 1
                case "covered":
                    covered_count += 1
                case _:
                    pass
            surrounding_tiles.update({(surrounding_row, surrounding_col) : surrounding_tile})

    return (flagged_count, covered_count, surrounding_tiles)

def find_adjacent_covered_tiles(tile_dict):
    if not(isinstance(tile_dict, dict)):
        return None

    coords_list = []

    # Only analyse the surrounding tiles which are COVERED
    for coord, value in tile_dict.items():
        if(value == "covered"):
            coords_list.append(coord)

    # Group Adjacent Coordinates
    # Using product() + groupby() + list comprehension
    man_tups = [sorted(sub) for sub in product(coords_list, repeat = 2)
                                            if Manhattan(*sub) == 1]

    res_dict = {ele: {ele} for ele in coords_list}
    for tup1, tup2 in man_tups:
        res_dict[tup1] |= res_dict[tup2]
        res_dict[tup2] = res_dict[tup1]

    res = [[*next(val)] for key, val in groupby(
            sorted(res_dict.values(), key = id), id)]


    return res
    
def find_surrounding_number_tiles(tile_dict):
    if not(isinstance(tile_dict, dict)):
        return None

    coords_list = []

    for coord, value in tile_dict.items():
        try:
            int(value) # "If string is an integer value, a number 1-8"
            coords_list.append(coord)
        except:
            continue


    return coords_list

def only_has_one_N_length_sublist(tile_list, N):
    # Ensure supplied list is 2D
    if not(isinstance(tile_list, list) and all(isinstance(row, list) for row in tile_list)):
        return False

    if(len(tile_list) == 1):
        if(len(tile_list[0]) == N):
            return True
    
    return False

def is_corner(tile, centre):
    y,x = centre
    return tile == (y+1,x+1) or tile == (y+1, x-1) or tile == (y-1, x+1) or tile == (y-1, x-1)

def Manhattan(tup1, tup2):
    return abs(tup1[0] - tup2[0]) + abs(tup1[1] - tup2[1])

while True:
    minefield_array = sweeperLib.process_grid()
    gridSize = sweeperLib.get_grid_size() # FIX THIS LINE BECAUSE IT IS OVERWRITING THE GRID SIZE NEEDLESSLY EVERY LOOP. FIND A WAY TO GET THIS VARIABLE ONCE, BUT ONLY AFTER gridSize HAS BEEN ALLOCATED A VALUE OTHER THAN (0,0) IN sweeperLib.py
    if minefield_array:
        step_solver(minefield_array)
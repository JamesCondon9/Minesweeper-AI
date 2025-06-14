import sweeperLib
from sweeperLib import MineNotWithinGridException
import time
from itertools import groupby, product

# Defines whether or not a pass of all mines resulted in no actions being taken, used to decide
# whether it is necessary to use reduction / mine counting patterns
no_click_pass = True
"""
Note - Using a global here is bad practice,
refactor in future if possible (more motivated
to spend my time making the core functionality
at this particular moment in time 😁)

^^^ NOT CURRENTLY WORKING, CONTINUE WORKING FROM HERE NEXT PROGRAMMING SESSION ^^^
"""

def step_solver(minefield):
    # Attempt to take a step??? Hope for the best maybe just click a random tile or something
    for y, row  in enumerate(minefield):
        for x, tile in enumerate(row):
            if(tile == "covered" or tile == "empty" or tile == "flagged"):
                continue # Skip non-number tiles.
            else:
                #print(f"\nStepping from row {y} column {x} tile...")
                solve_from_tile(minefield, x, y)

    global no_click_pass
    no_click_pass = True

def solve_from_tile(minefield, x, y):
    
    global no_click_pass

    # TEMPORARY TEST BLOCK ~~~~~~
    """ if(y!= 6 or x != 4):
        return """
    # TEMPORARY TEST BLOCK ~~~~~~

    try:
        tile_value = int(minefield[y][x])
    except:
        raise Exception("Error - basic_checks function should only be called on tiles that have number values")
    
    try:
        flagged_count, covered_count, surrounding_tiles = find_surrounding_tiles(minefield, y, x)
    except TypeError: # If nothing is returned from find_surrounding_tiles
        return

    touching_count = flagged_count + covered_count
    surrounding_adjacent_covered_tiles = []
    surrounding_number_tiles = {}

    #print(f"covered: {covered_count} flagged: {flagged_count} touching: {touching_count}")

    # Check if number tile is already solved, and if so, ignore it.
    if(tile_value == flagged_count and covered_count == 0):
        return

    # Basic Pattern [B1]
    
    if(tile_value-flagged_count == covered_count):
        print("BASIC CHECK [B1] ACTIVATED!")
        for (row, col), value in surrounding_tiles.items(): # Flag all covered tiles as they are all mines
            if(value == "covered"):
                no_click_pass = False
                sweeperLib.flag_tile((col, row))
                minefield[row][col] = "flagged"
        return
    
    # Basic Pattern [B2]
    elif(tile_value == flagged_count):
        print("BASIC CHECK [B2] ACTIVATED!")
        for (row, col), value in surrounding_tiles.items(): # Click all covered tiles as there are no remaining mines
            if(value == "covered"):
                no_click_pass = False
                sweeperLib.uncover_tile((col, row))
        return

    
    else: # Prepare more data for next round of basic checks on the tile
        surrounding_adjacent_covered_tiles = find_adjacent_covered_tiles(surrounding_tiles)
        surrounding_number_tiles = find_surrounding_number_tiles(surrounding_tiles)

    # Basic Patterns [1-1], [1-1+], [1-2] and [1-2+]

    if(tile_value == 1 and only_has_one_N_length_sublist(surrounding_adjacent_covered_tiles, 2)):
        tile_index = 0 # Used to pick "the other one" out of the list of two coordinates, i.e. from the corner tile, used to find the index of the edge tile with a mod operation
        for tile in surrounding_adjacent_covered_tiles[0]:
            tile_index += 1
            # Only process the tile of the pair that is the corner tile (this will tell us where the second number tile is located)
            if(is_corner(tile, (y,x))):
                edge_tile = surrounding_adjacent_covered_tiles[0][tile_index % 2]

                delta_y = tile[0] - edge_tile[0]
                delta_x = tile[1] - edge_tile[1]

                new_tile_value = minefield[y+delta_y][x+delta_x]

                try:
                    int(new_tile_value)
                except:
                    return

                # [1-1] and [1-1+]
                if(new_tile_value == "1"):
                    _, _, new_surrounding_tiles = find_surrounding_tiles(minefield, y+delta_y, x+delta_x)

                    # Click all tiles that aren't the original two adjacent ones
                    for new_surrounding_tile in new_surrounding_tiles:
                        # Check it wasn't one of the originals and is a covered tile
                        if(not new_surrounding_tile in surrounding_tiles and minefield[new_surrounding_tile[0]][new_surrounding_tile[1]] == "covered"):
                            try:
                                no_click_pass = False
                                sweeperLib.uncover_tile((new_surrounding_tile[1], new_surrounding_tile[0]))
                            except MineNotWithinGridException as e:
                                print(f"Invalid mine coordinate: {e}")
                    
                    return

                # [1-2] and [1-2+]
                elif(int(new_tile_value) >= 2 and int(new_tile_value) <= 8):
                    flagged_count, covered_count, new_surrounding_tiles = find_surrounding_tiles(minefield, y+delta_y, x+delta_x)

                    # Remaining tiles must be mines if there is exactly one mine in the original two adjacent tiles
                    if(covered_count-2 == int(new_tile_value)-flagged_count-1):
                        # Flag all remaining unflagged tiles that are not in the original two adjacent tiles
                        for new_surrounding_tile in new_surrounding_tiles:
                            # Check it wasn't one of the originals and is a covered tile
                            if(not new_surrounding_tile in surrounding_tiles and minefield[new_surrounding_tile[0]][new_surrounding_tile[1]] == "covered"):
                                try:
                                    no_click_pass = False
                                    minefield[new_surrounding_tile[0]][new_surrounding_tile[1]] = "flagged"
                                    sweeperLib.flag_tile((new_surrounding_tile[1], new_surrounding_tile[0]))
                                except MineNotWithinGridException as e:
                                    print(f"Invalid mine coordinate: {e}")
                    return

    # Basic Patterns [1-2-1] and [1-2-2-1]
    # Hole Patterns [H1]
    if(tile_value == 1):
        for (row, col), value in surrounding_number_tiles.items(): # Loop over four directions from 1st tile
            delta_y = row - y
            delta_x = col - x
            if(delta_y != 0 and delta_x != 0): # Ignore corner tiles
                continue

            if(value == "1"): # Keep checking for Hole Pattern [H1]
                offset_x = 0 if delta_y == 0 else 1 # i.e. set to 1 when delta_x == 0
                offset_y = 1 if delta_y == 0 else 0

                common_tile_one = (y+delta_y+offset_y,x+delta_x+offset_x)
                common_tile_two = (y+delta_y+-1*offset_y,x+delta_x+-1*offset_x)

                if(not(is_in_bounds(common_tile_one[0], common_tile_two[1]) or is_in_bounds(common_tile_two[0], common_tile_two[1]))):
                   continue

                if(coords_are_only_surroundings(minefield, [common_tile_one, common_tile_two], y, x)):
                    _, _, new_surrounding_tiles = find_surrounding_tiles(minefield, y+delta_y, x+delta_x)
                    for coord, value in new_surrounding_tiles.items():
                        print(f"HOLE CHECK [H1] ACTIVATED AT {y,x}!")
                        if(value == "covered" and coord != common_tile_one and coord != common_tile_two and is_in_bounds(coord[0], coord[1])):
                            no_click_pass = False
                            sweeperLib.uncover_tile(coord)
                
                
                
                return

            elif(value == "2"): # Keep checking for Basic Patterns [1-2-1] and [1-2-2-1]
                # Check 3rd tile is in-bounds
                if(not(is_in_bounds(row+2*delta_y, col+2*delta_x))):
                        continue

                next_tile_value = minefield[row+delta_y][col+delta_x] # 3rd tile

                if(next_tile_value == "1"): # [1-2-1]

                    print("BASIC CHECK [1-2-1] ACTIVATED!")
                    # Find where the edge tiles to click are based on [1-2-1] line direction
                    if(delta_y == 0):
                        offset_x = 0
                        if(minefield[y-1][x] == "covered"):
                            offset_y = -1
                        else:
                            offset_y = 1
                    else: # (delta_x == 0)
                        if(minefield[y][x-1] == "covered"):
                            offset_x = -1
                        else:
                            offset_x = 1
                        offset_y = 0

                    # Define the covered tiles adjacent to the [1-2-1] pattern
                    tile0 = (y - delta_y + offset_y, x - delta_x + offset_x)       # Down+Left from first [1]
                    tile1 = (y + offset_y, x + offset_x)                           # Down from first [1]
                    tile2 = (y + delta_y + offset_y, x + delta_x + offset_x)       # Down from first [2]
                    tile3 = (y + 2 * delta_y + offset_y, x + 2 * delta_x + offset_x)  # Down from second [1]
                    tile4 = (y + 3 * delta_y + offset_y, x + 3 * delta_x + offset_x)  # Down+Right from second [1]

                    # Check there are not tiles adjacent to the [1-2-1] that invalidate the pattern (only one parallel line allowed)
                    if not coords_are_only_surroundings(minefield, [tile0, tile1, tile2], y, x):
                        continue
                    if not coords_are_only_surroundings(minefield, [tile1, tile2, tile3], y, x):
                        continue
                    if not coords_are_only_surroundings(minefield, [tile2, tile3, tile4], y, x):
                        continue

                    if(minefield[y+offset_y][x+offset_x] == "covered"):
                        no_click_pass = False
                        sweeperLib.flag_tile((x+offset_x, y+offset_y))
                        minefield[y+offset_y][x+offset_x] = "flagged"

                    if(minefield[y+delta_y+offset_y][x+delta_x+offset_x] == "covered"):
                        no_click_pass = False
                        sweeperLib.uncover_tile((x+delta_x+offset_x, y+delta_y+offset_y))

                    if(minefield[y+2*delta_y+offset_y][x+2*delta_x+offset_x] == "covered"):
                        no_click_pass = False
                        sweeperLib.flag_tile((x+2*delta_x+offset_x, y+2*delta_y+offset_y))
                        minefield[y+2*delta_y+offset_y][x+2*delta_x+offset_x] = "flagged"

                    return

                elif(next_tile_value == "2"): # Check further for [1-2-2-1]
                    # Check 4th tile is in-bounds
                    if(not(is_in_bounds(row+3*delta_y, col+3*delta_x))):
                            continue

                    next_tile_value = minefield[y+3*delta_y][x+3*delta_x] # 4th tile
                    if(next_tile_value == "1"): # [1-2-2-1]
                        print("BASIC CHECK [1-2-2-1] ACTIVATED!")

                        # Find where the edge tiles to click are based on [1-2-2-1] line direction
                        if(delta_y == 0):
                            offset_x = 0
                            if(minefield[y-1][x] == "covered"):
                                offset_y = -1
                            else:
                                offset_y = 1
                        else: # (delta_x == 0)
                            if(minefield[y][x-1] == "covered"):
                                offset_x = -1
                            else:
                                offset_x = 1
                            offset_y = 0

                        # Define the covered tiles adjacent to the [1-2-2-1] pattern
                        tile0 = (y - delta_y + offset_y, x - delta_x + offset_x)       # Down+Left from first [1]
                        tile1 = (y + offset_y, x + offset_x)                           # Down from first [1]
                        tile2 = (y + delta_y + offset_y, x + delta_x + offset_x)       # Down from first [2]
                        tile3 = (y + 2 * delta_y + offset_y, x + 2 * delta_x + offset_x)  # Down from second [2]
                        tile4 = (y + 3 * delta_y + offset_y, x + 3 * delta_x + offset_x)  # Down from second [1]
                        tile5 = (y + 4 * delta_y + offset_y, x + 4 * delta_x + offset_x)  # Down+Right from second [1]

                        # Check there are not tiles adjacent to the [1-2-2-1] that invalidate the pattern (only one parallel line allowed)
                        if not coords_are_only_surroundings(minefield, [tile0, tile1, tile2], y, x): # Around first [1]
                            continue
                        if not coords_are_only_surroundings(minefield, [tile1, tile2, tile3], y, x): # Around first [2]
                            continue
                        if not coords_are_only_surroundings(minefield, [tile2, tile3, tile4], y, x): # Around second [2]
                            continue
                        if not coords_are_only_surroundings(minefield, [tile3, tile4, tile5], y, x): # Around second [1]
                            continue

                        if(minefield[y+offset_y][x+offset_x] == "covered"):
                            no_click_pass = False
                            sweeperLib.uncover_tile((x+offset_x, y+offset_y))

                        if(minefield[y+delta_y+offset_y][x+delta_x+offset_x] == "covered"):
                            no_click_pass = False
                            sweeperLib.flag_tile((x+delta_x+offset_x, y+delta_y+offset_y))
                            minefield[y+delta_y+offset_y][x+delta_x+offset_x] = "flagged"

                        if(minefield[y+2*delta_y+offset_y][x+2*delta_x+offset_x] == "covered"):
                            no_click_pass = False
                            sweeperLib.flag_tile((x+2*delta_x+offset_x, y+2*delta_y+offset_y))
                            minefield[y+2*delta_y+offset_y][x+2*delta_x+offset_x] = "flagged"

                        if(minefield[y+3*delta_y+offset_y][x+3*delta_x+offset_x] == "covered"):
                            no_click_pass = False
                            sweeperLib.uncover_tile((x+3*delta_x+offset_x, y+3*delta_y+offset_y))

                        return


    # Generate frontier
    """
    Note - We generate the frontier if a pass has happened with
    no clicks to improve efficiency. With reduction / mine counting,
    it's particularly impactful to only consider frontier tiles as
    it avoids generating pairs of tiles that will lead to nothing.
    """
    if(no_click_pass): # Only move onto reduction and mine counting when the solver gets stuck using previous patterns
        frontier = find_frontier(minefield)
        print(frontier)
    else:
        return # If we didn't just have a pass where nothing happened, keep going with the previous checks.

    """ ^^^ NOT CURRENTLY WORKING, CONTINUE WORKING FROM HERE NEXT PROGRAMMING SESSION ^^^ """

    # Reduction
    

    # Mine Counting
    

def find_frontier(minefield):
    frontier = {}

    for frontier_y, frontier_row  in enumerate(minefield):
        for frontier_x, frontier_tile in enumerate(frontier_row):
            if(frontier_tile == "covered" or frontier_tile == "empty" or frontier_tile == "flagged"):
                continue # Skip non-number tiles.
            else:
                # Only add number tiles that have at least one surrounding covered tile
                _, _, frontier_surrounding_tiles = find_surrounding_tiles(minefield, frontier_y, frontier_x)
                surrounding_covered_tiles = find_surrounding_covered_tiles(frontier_surrounding_tiles)
                if(surrounding_covered_tiles):
                    frontier.update(surrounding_covered_tiles)

    return frontier

def find_surrounding_tiles(minefield, y, x):
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

            if(not(is_in_bounds(surrounding_row, surrounding_col))): # Check surrounding mines are not on the edge of the board
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
    
def find_surrounding_covered_tiles(tile_dict):
    if not(isinstance(tile_dict, dict)):
        return None

    coords_dict = {}

    for coord, value in tile_dict.items():
        if(value == "covered"):
            coords_dict.update({coord:value})

    return coords_dict
    
def find_surrounding_number_tiles(tile_dict):
    if not(isinstance(tile_dict, dict)):
        return None

    coords_dict = {}

    for coord, value in tile_dict.items():
        try:
            int(value) # "If string is an integer value, a number 1-8"
            coords_dict.update({coord:value})
        except:
            continue


    return coords_dict

def only_has_one_N_length_sublist(tile_list, N):
    # Ensure supplied list is 2D
    if not(isinstance(tile_list, list) and all(isinstance(row, list) for row in tile_list)):
        return False

    if(len(tile_list) == 1):
        if(len(tile_list[0]) == N):
            return True
    
    return False

def coords_are_only_surroundings(minefield, coords, row, col): # Check supplied coords list are the only covered tiles surrounding (y,x)
    _, _, surrounding_tiles = find_surrounding_tiles(minefield, row, col)
    for tile, value in surrounding_tiles.items():
        if(not(tile in coords) and value == "covered"):
            return False
    return True

def is_corner(tile, centre):
    y,x = centre
    return tile == (y+1,x+1) or tile == (y+1, x-1) or tile == (y-1, x+1) or tile == (y-1, x-1)

def is_in_bounds(row, col):
    return not(row > gridSize[0]-1 or col > gridSize[1]-1 or row < 0 or col < 0)

def Manhattan(tup1, tup2):
    return abs(tup1[0] - tup2[0]) + abs(tup1[1] - tup2[1])

while True:
    minefield_array = sweeperLib.process_grid()
    gridSize = sweeperLib.get_grid_size() # FIX THIS LINE BECAUSE IT IS OVERWRITING THE GRID SIZE NEEDLESSLY EVERY LOOP. FIND A WAY TO GET THIS VARIABLE ONCE, BUT ONLY AFTER gridSize HAS BEEN ALLOCATED A VALUE OTHER THAN (0,0) IN sweeperLib.py
    if minefield_array:
        step_solver(minefield_array)
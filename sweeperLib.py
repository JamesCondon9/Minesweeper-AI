"""
Minesweeper utility module used to:
> Screenshot the grid
> Convert it into a numerical representation
> Uncover / flag tiles
"""

import cv2
import pyautogui as pag
import numpy as np
from pyautogui import ImageNotFoundException

DIFFICULTY = 2 # 1: Beginner (9,9)  2: Intermediate (16,16)  3: Expert (16,30)
monitorX = 1920
monitorY = 1080
TILE_SIZE = 32
gridSize = (0,0)
topLeftCellCentre = (0,0)

tile_templates = {
    "1": cv2.imread('./sweeperLibAssets/One Tile.png', cv2.IMREAD_GRAYSCALE),
    "2": cv2.imread('./sweeperLibAssets/Two Tile.png', cv2.IMREAD_GRAYSCALE),
    "3": cv2.imread('./sweeperLibAssets/Three Tile.png', cv2.IMREAD_GRAYSCALE),
    "4": cv2.imread('./sweeperLibAssets/Four Tile.png', cv2.IMREAD_GRAYSCALE),
    "5": cv2.imread('./sweeperLibAssets/Five Tile.png', cv2.IMREAD_GRAYSCALE),
    "6": cv2.imread('./sweeperLibAssets/Six Tile.png', cv2.IMREAD_GRAYSCALE),
    "7": cv2.imread('./sweeperLibAssets/Seven Tile.png', cv2.IMREAD_GRAYSCALE),
    "8": cv2.imread('./sweeperLibAssets/Eight Tile.png', cv2.IMREAD_GRAYSCALE),
    "empty": cv2.imread('./sweeperLibAssets/Empty Tile.png', cv2.IMREAD_GRAYSCALE),
    "covered": cv2.imread('./sweeperLibAssets/Covered Tile.png', cv2.IMREAD_GRAYSCALE),
    "flagged": cv2.imread('./sweeperLibAssets/Flag Tile.png', cv2.IMREAD_GRAYSCALE)
}

class MineGridNotFullyVisibleException(Exception):
    """Raised when the mine grid is not fully visible in the screenshot."""
    pass

class MineNotWithinGridException(Exception):
    """Raised when the coordinates provided are outside the grid size."""
    pass

class TemplateLoadException(Exception):
    """Raised when a tile template fails to load properly."""
    pass

class UnidentifiableTileException(Exception):
    """Raised when a tile does not match to any of the templates"""
    pass

def process_grid():

    try:

        # ~~ PRE-SCREENSHOT CODE ~~

        anchorX, anchorY = locate_anchor()

        global gridSize
        global topLeftCellCentre

        match DIFFICULTY: # Set top left cell centre and acceptable mine grid borders based on difficulty (grid size)
            case 1: # Beginner difficulty (9,9)
                gridSize = (9,9)
                rightXBorder = anchorX - 115
                leftXBorder = anchorX + 170
                downYBorder = anchorY + 380
                topLeftCell = (anchorX - 125, anchorY + 48)
                
            case 2: # Intermediate difficulty (16,16)
                gridSize = (16,16)
                rightXBorder = anchorX - 225
                leftXBorder = anchorX + 285
                downYBorder = anchorY + 615
                topLeftCell = (anchorX - 238, anchorY + 48)
            case 3: # Expert difficulty (16,30)
                gridSize = (16,30)
                rightXBorder = anchorX - 460
                leftXBorder = anchorX + 500
                downYBorder = anchorY + 615
                topLeftCell = (anchorX - 462, anchorY + 48)
            case _:
                raise Exception

        topLeftCellCentre = (topLeftCell[0] + 15, topLeftCell[1] + 15)

        # Ensure entire mine grid is FULLY visible on screen, ready to analyse and click
        if(not(rightXBorder > 0 and
            leftXBorder < monitorX and
            downYBorder < monitorY)):
                raise MineGridNotFullyVisibleException("Mine grid is not fully visible on the screen")

        # ~~ TILE IMAGE PROCESSING ~~

        # Classify each of the tiles of the grid from the screenshot

        screenshot =  pag.screenshot(region=(int(topLeftCell[0]), int(topLeftCell[1]), gridSize[1]*32, gridSize[0]*32),)
        if(screenshot == None):
            return 
        
        minefieldArray = np.array(screenshot)
        minefieldGray = cv2.cvtColor(minefieldArray, cv2.COLOR_RGB2GRAY)

        tileGrid = [] # 2D Array of minefield: tileGrid[row][column]

        for row in range(0, gridSize[0]):
            rowTiles = []
            for column in range(0, gridSize[1]):
                x = column * TILE_SIZE
                y = row * TILE_SIZE
                tile = minefieldGray[y+1:y+TILE_SIZE, x+1:x+TILE_SIZE]
                tile_class = classify_tile(tile)
                if(tile_class == None):
                    raise UnidentifiableTileException
                else:
                    rowTiles.append(tile_class)
            tileGrid.append(rowTiles)

        return tileGrid

    except ImageNotFoundException:
        #print("Minesweeper game not currently in view")
        pass

    except MineGridNotFullyVisibleException as e:
        print(f"Error: {e}")

    except TemplateLoadException as e:
        print(f"Tile template loading failed: {e}")

    except MineNotWithinGridException as e:
        print(f"Invalid mine coordinate: {e}")

    except UnidentifiableTileException as e:
        print(f"Tile could not be identified: {e}")

    except Exception as e:
        print(f"An unexpected error occurred: {e}")
    
def locate_anchor(ignoreSection=250, monitorX=1920, monitorY=1080, anchorImage='./sweeperLibAssets/greyscaleMonkaHmm.png'):
    # Locate top left of anchor image
    screenshotRegion = (ignoreSection, ignoreSection, monitorX-(2*ignoreSection), monitorY-(2*ignoreSection)) # Only screenshot central portion of screen to save compute time
    pepeLocation = pag.locateOnScreen(anchorImage, region=screenshotRegion, grayscale=True , confidence=0.5) # Region and greyscale improve performance
    (anchorX, anchorY, _, _) = pepeLocation
    return (anchorX, anchorY)

def classify_tile(tile_gray):
    for label, template in tile_templates.items():
        if template is None:
            raise TemplateLoadException("A template used to match tile types was loaded incorrectly")

        res = cv2.matchTemplate(tile_gray, template, cv2.TM_CCOEFF_NORMED)
        _, val, _, _, = cv2.minMaxLoc(res)
        if(val > 0.90):
            return label

def uncover_tile(solverGridCoords):
    if(solverGridCoords[1] > gridSize[0]-1 or solverGridCoords[0] > gridSize[1]-1):
        raise MineNotWithinGridException(f"Trying to uncover tile {(solverGridCoords)} which is out of bounds.")

    pag.moveTo(topLeftCellCentre[0] + solverGridCoords[0]*32, topLeftCellCentre[1] + solverGridCoords[1]*32)
    pag.leftClick(_pause=False)
    
def flag_tile(solverGridCoords): # solverGridCoords: (column, row)
    if(solverGridCoords[1] > gridSize[0]-1 or solverGridCoords[0] > gridSize[1]-1):
        raise MineNotWithinGridException(f"Trying to flag tile {(solverGridCoords)} which is out of bounds.")

    pag.moveTo(topLeftCellCentre[0] + solverGridCoords[0]*32, topLeftCellCentre[1] + solverGridCoords[1]*32)
    pag.rightClick(_pause=False)

def get_grid_size():
    return gridSize
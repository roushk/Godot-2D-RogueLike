import json
import os

# Usage: Run py .\GeneratePartsListFromAssets.py
# Copy parts that you want from OutputParts.json into \Data\PartsList.json

# Create data
data = []
directory = "Parts"
#Iterate over .png files because they follow a naming schema so use that to create json to save time
for filename in os.listdir(directory):
    if filename.endswith(".png") : 
        #Get the location of the split between the part type and part name
        partName = filename.rfind("_")
        data.append({
          #genereate substr from partname + 1 to end of str - 4 for the .png string
        'partName': filename[partName + 1:len(filename) - 4],
        'partTextureDir': 'res://Assets/Art/My_Art/Parts/' + filename,
          #genereate substr from partname + 1 to end of str
        'partType': filename[0:partName],
          #basic part cost
        'partCost': 5
    })
        continue
    else:
        continue

with open('OutputParts.json', 'w') as outfile:
  json.dump(data, outfile)

import re
import csv
import sys

gEPCFieldNames = ["EPC"]

def updateStorageTable( csvStorageFName, epcSet ):
      with open(csvStorageFName, "w") as csvFile:
            writer = csv.DictWriter(csvFile, fieldnames=gEPCFieldNames)
            writer.writeheader()
            for items in epcSet:
                  writer.writerow({gEPCFieldNames[0]:items})
            csvFile.close()
      return
      

def storeNewValues( newValueFName, csvStorageFName ):
      epcList = getEPCList(csvStorageFName)
      epcSet = createSetFromStorageTable(epcList)
      print(epcSet)
      with open(newValueFName, 'r') as txtIn:
            newEntriesList = []
            for entries in txtIn:
                  values = parseNewEntry(entries)
                  if len(values) >= 1:
                        newEntriesList.append(values[0])
            txtIn.close()
      newEntriesSet = set(newEntriesList)
      epcSet.update(newEntriesSet)
      print(epcSet)
      updateStorageTable(csvStorageFName, epcSet)
      return

def parseNewEntry( entry ):
      categories = re.split("(\s+)", entry)
      fields = []
      for phrases in categories:
            words = re.split("(:)", phrases)
            if verifyPhrase(words):
                  fields.append(words[2])
      return fields

def verifyPhrase( words ):
      if len(words) < 3:
            return False
      elif words[1] != ":":
            return False
      return True

def getEPCList( csvStorageFName ):
      epcs = []
      try:
            with open(csvStorageFName, "r") as csvStorageTable:
                  reader = csv.DictReader(csvStorageTable, fieldnames=gEPCFieldNames)
                  index = 0
                  for row in reader:
                        # skip the header!
                        if index > 0:
                              epcs.append(row["EPC"])
                        index = index + 1

                  csvStorageTable.close()
      except:
            e = sys.exc_info()[0]
            print(e)
      return epcs

def createSetFromStorageTable( EPCList ):
      epc = set(EPCList)
      return epc

# run the process
storeNewValues("readOut.txt", "storageTable.csv")


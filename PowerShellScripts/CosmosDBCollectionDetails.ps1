#Install the module if you haven't aleardy
#Install-Module ImportExcel -AllowClobber -Force

<#
ToDo: below line will connect to Azure, login will popup, provide required information 
#>
Connect-AzAccount

# Create an Excel object
$ExcelObj = New-Object -comobject Excel.Application 

# Add a workbook
$ExcelWorkBook = $ExcelObj.Workbooks.Add()
$ExcelWorkSheet = $ExcelWorkBook.Worksheets.Item(1)
# Rename a worksheet
$ExcelWorkSheet.Name = 'CollectionsDetails'

# Fill in the head of the table
$ExcelWorkSheet.Cells.Item(1,1) = 'Susbcription ID'
$ExcelWorkSheet.Cells.Item(1,2) = 'CosmosDB Account'
$ExcelWorkSheet.Cells.Item(1,3) = 'Database Name'
$ExcelWorkSheet.Cells.Item(1,4) = 'Collection Name'
$ExcelWorkSheet.Cells.Item(1,5) = 'Auto Scale'
$ExcelWorkSheet.Cells.Item(1,6) = 'Min Throughput'
$ExcelWorkSheet.Cells.Item(1,7) = 'Max Throughput'

$counter=2 #Counter for writing to Excel 

<#
ToDo: set your teanantID details
#>
$tenantID= "aaaa-aaaa-aaaa-aaaa"
$subscriptions = Get-AzSubscription -TenantId $tenantID 

foreach ($subscriptionID in $subscriptions.Id)
{
    Select-AzSubscription -SubscriptionId $subscriptionID 

    $resourceGroupNames= Get-AzResourceGroup 

    foreach ($resourceGroupName in $resourceGroupNames.ResourceGroupName) {
   
        #Retrieve database accounts
        $databaseAccounts = Get-AzCosmosDBAccount -ResourceGroupName $resourceGroupName
        foreach($databaseAccount in $databaseAccounts ){
          
     
            #Retrieve databases
            $databases = Get-AzCosmosDBSqlDatabase -ResourceGroupName $resourceGroupName -AccountName $databaseAccount.Name
    
    
            foreach($database in $databases.Name ){                    
                #Retrieve collections
                $collections = Get-AzCosmosDBSqlContainer -ResourceGroupName $resourceGroupName -AccountName $databaseAccount.Name -DatabaseName $database
            
                foreach($collection in $collections.Name){
                
                    $throughput = Get-AzCosmosDBSqlContainerThroughput -ResourceGroupName $resourceGroupName -AccountName $databaseAccount.Name -DatabaseName $database -Name $collection
                    $autoscaleSettings = $throughput.AutoscaleSettings
                    $minThroughput = $throughput.MinimumThroughput
                    $maxtargetThroughput= $autoscaleSettings.TargetMaxThroughput

                    $ExcelWorkSheet.Cells.Item($counter,1) = $subscriptionID
                    $ExcelWorkSheet.Cells.Item($counter,2) = $databaseAccount.Name
                    $ExcelWorkSheet.Cells.Item($counter,3) = $database
                    $ExcelWorkSheet.Cells.Item($counter,4) = $collection

                    if ($autoscaleSettings.MaxThroughput -eq 0) {
                        $ExcelWorkSheet.Cells.Item($counter,5) = "0" #not enabled
                    } else {
                      $ExcelWorkSheet.Cells.Item($counter,5) = "1" #enabled
                      $ExcelWorkSheet.Cells.Item($counter,7) = $autoscaleSettings.MaxThroughput
                    }
                    $ExcelWorkSheet.Cells.Item($counter,6) = $minThroughput
                    


                    $counter++
                }#colls 
            }#dbs
        }#acc

    }#rsg
}#sub

<#
ToDo: Update the file details, where you want to save collection details
#>

$FilePath= "c:\Microsoft\CosmosDB Details.xlsx"
If (test-path $FilePath) {Remove-Item $FilePath}

# Save the XLS file and close Excel
$ExcelWorkBook.SaveAs($FilePath)
$ExcelWorkBook.close($true)



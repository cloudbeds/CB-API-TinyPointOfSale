# CB-API-TinyPointOfSale
This project contains sample code (C#) for an application demonstrating how to use the APIs needed for Point of Sale application that integrates with Cloudbeds.

1. To use this application you will need to FIRST use the **CB-API-Explorer** (https://github.com/cloudbeds/CB-API-Explorer) sample to create an XML file that contains the access tokens (i.e. secrets) this application uses to log into your Cloudbeds property:

    i. Load the CB-API-Explorer sample and run it
    
    ii. Click on the "Bootstrap to create user access tokens" button and follow the instructions
    
    iii. Click on the "Save the user access tokens to storage" button to store the access tokens in a local file
    
    **Result:** You will not be able to run THIS (CB-API-TinyPointOfSale) application and it will use these secrets to log into your Cloudbeds property
   
2. This application implements a VERY SIMPLE Point of Sale system:

     i. You can choose menu items to add to a guest's bill
     
     ii. You can query Cloudbeds for the list of current Guests registered in your property, and select a guest to assign the bill to
     
     iii. The guest can add an optional gratuity to the bill
     
     iv. You can submit this bill to Cloudbeds to add to the guest's folio as a charge.
     
         - All the menu items added to the guest’s bill are stored as line items in the guest's folio
         
         - The notes field in each line item is written to show that the items belong grouped together
         
         - A unique ID is stored with the folio entry to prevent duplicate submissions of the same bill
         
       

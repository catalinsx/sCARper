document.addEventListener("DOMContentLoaded", () => {
    const brandDropdown = document.getElementById("brandDropdown");
    const modelDropdown = document.getElementById("modelDropdown");
    const searchButton = document.getElementById('searchButton');
    
    
    // fetching brand and model from json file, if it's not working, throw an error
    fetch("car-list.json")
        .then(response => {
            if (!response.ok) {
                throw new Error("Failed to fetch car list json");
            }
            return response.json();
        })
        .then(data => {
            // sort alfabetic la branduri
            data.sort((a, b) => a.brand.localeCompare(b.brand));

            // a loop that iterates through the sorted list and populate the brand dropdown
            data.forEach(item => {
                const option = document.createElement("option");
                option.value = item.brand;
                option.textContent = item.brand;
                brandDropdown.appendChild(option);
            });

           
            // when the user selects a brand from the brandDropdown, the event is triggered
            // the model dropdown is now available and i made a function to find the corresponding models for the selected brand
            brandDropdown.addEventListener("change", () => {
                modelDropdown.innerHTML = "<option value='' disabled selected>Selectează Model</option>";
                const selectedBrand = brandDropdown.value;

      
                const models = data.find(item => item.brand === selectedBrand)?.models || [];
                models.forEach(model => {
                    const option = document.createElement("option");
                    option.value = model;
                    option.textContent = model;
                    modelDropdown.appendChild(option);
                });

    
                modelDropdown.disabled = models.length === 0;
                
                //update the state of searchbutton
                checkForm();
            });

        
        })
        .catch((error) => {
            alert(error);
        });


    function checkForm() {
   
        if (brandDropdown.value && modelDropdown.value) {
            searchButton.disabled = false; 
        } else {
            searchButton.disabled = true; 
        }
    }
    modelDropdown.addEventListener("change", checkForm);
});





let selectedExchanges = [];
let selectedAssets = [];
let selectedUnderlyings = [];

let derivativeData = {};
let underlyingData = {};
let tradeData = {};


$(document).ready(function () {
    // defult Exchange page
    fetchExchangeData();

    $('#exchange-btn').click(function () {
        showExchange();
    });

    $('#asset-btn').click(function () {
        showAsset();
    });

    $('#underlying-btn').click(function () {
        showUnderlying();
    });

    $('#derivative-btn').click(function () {
        showDerivative();
    });

    $('#rate-btn').click(function () {
        showRate();
    });

    $('#trade-btn').click(function () {
        showTrade();
    });

});


// show exchange
function showExchange() {
    $('#exchange-container').show();
    $('#asset-container').hide();
    $('#underlying-container').hide();
    $('#derivative-container').hide();
    $('#rate-container').hide();
    $('#trade-container').hide();
    fetchExchangeData();
}

// show asset
function showAsset() {
    $('#exchange-container').hide();
    $('#asset-container').show();
    $('#underlying-container').hide();
    $('#derivative-container').hide();
    $('#rate-container').hide();
    $('#trade-container').hide();
    fetchAssetData(selectedExchanges);
}

// show underlying
function showUnderlying() {
    $('#exchange-container').hide();
    $('#asset-container').hide();
    $('#underlying-container').show();
    $('#derivative-container').hide();
    $('#rate-container').hide();
    $('#trade-container').hide();
    fetchUnderlyingData(selectedAssets);
}

// show derivative
function showDerivative() {
    $('#exchange-container').hide();
    $('#asset-container').hide();
    $('#underlying-container').hide();
    $('#derivative-container').show();
    $('#rate-container').hide();
    $('#trade-container').hide();
    fetchDerivativeData(selectedUnderlyings);
}

// show rate
function showRate() {
    $('#exchange-container').hide();
    $('#asset-container').hide();
    $('#underlying-container').hide();
    $('#derivative-container').hide();
    $('#rate-container').show();
    $('#trade-container').hide();
    fetchRateCurveData();
}

// show trade
function showTrade() {
    $('#exchange-container').hide();
    $('#asset-container').hide();
    $('#underlying-container').hide();
    $('#derivative-container').hide();
    $('#rate-container').hide();
    $('#trade-container').show();
    fetchTradeData();
}


// fetch exchange
function fetchExchangeData() {
    fetch('http://localhost:5209/api/exchange')
        .then(response => {
            if (!response.ok) throw new Error('Unable to retrieve Exchange data');
            return response.json();
        })
        .then(data => {
            console.log("Exchange Data from API:", data);
            renderExchangeTable(data);
        })
        .catch(error => {
            alert('Unable to load Exchange data: ' + error.message);
            console.error(error);
        });
}

// render exchange
function renderExchangeTable(data) {
    
    // empty tableBody ensure repeat render 
    const tableBody = $('#exchange-data-body');
    tableBody.empty();

    // if data contains data
    if (data.length) {
        data.forEach(item => {
            tableBody.append(`
                <tr>
                    <td><input type="checkbox" class="exchange-checkbox" data-id="${item.exchangeId}"></td>
                    <td>${item.exchangeId}</td>
                    <td>${item.exchangeName}</td>
                    <td>${item.location}</td>
                    <td>${item.country}</td>
                    <td>${item.currency}</td>
                </tr>
            `);
        });
        $('.exchange-checkbox').change(function () {
            selectedExchanges = getSelectedExchangeIds();
        });

    } else {
        tableBody.append(`
            <tr>
                <td colspan="6" style="text-align: center;">No data</td>
            </tr>
        `);
    }
}


function fetchAssetData(selectedExchanges = []) {
    fetch('http://localhost:5209/api/asset')
        .then(response => {
            if (!response.ok) throw new Error('Unable to retrieve Asset data');
            return response.json();
        })
        .then(data => {
            let filteredData;

            // if selected exchange
            if (selectedExchanges.length) {
                
                // filter
                filteredData = data.filter(item => selectedExchanges.includes(item.exchangeId));

                console.log('Filtered Data:', filteredData);
                console.log('Data after filter:', filteredData); 

                renderAssetTable(filteredData);
            } 

            else {
                // return full data
                filteredData = data;

                console.log('Data after no filter:', filteredData);

                renderAssetTable(filteredData);
            }
        })
        .catch(error => {
            alert('Unable to load Asset data: ' + error.message);
            console.error(error);
        });
}

// render asset
function renderAssetTable(data) {
    const tableBody = $('#asset-data-body');
    tableBody.empty();
    if (data.length) {
        data.forEach(item => {
            tableBody.append(`
                <tr>
                    <td><input type="checkbox" class="asset-checkbox" data-id="${item.assetType}"></td>
                    <td>${item.assetType}</td>
                    <td>${item.explanation}</td>
                    <td>${item.exchangeId}</td>
                </tr>
            `);
        });
        $('.asset-checkbox').change(function () {
            selectedAssets = getSelectedAssets();
            console.log('selectedAssets:', selectedAssets)
        });
    } else {
        tableBody.append(`
            <tr>
                <td colspan="3" style="text-align: center;">No Data</td>
            </tr>
        `);
    }
}

// check box function
function getSelectedExchangeIds() {
    const selectedIds = [];
    $('.exchange-checkbox:checked').each(function () {
        selectedIds.push($(this).data('id'));
    });
    return selectedIds;
}

// filter function
$('#filter-assets-btn').click(function () {
    const selectedExchanges = getSelectedExchangeIds();

    // if there is selection
    if (selectedExchanges.length) {
        
        fetchAssetData(selectedExchanges);
    
    // if no selection
    } else {
        fetchAssetData();
    }
    showAsset();
});

// clear filter function
$('#clear-filter-btn').click(function () {
    selectedExchanges = [];
    console.log('Filter cleared. Selected Exchanges:', selectedExchanges);
    fetchAssetData();
    showAsset();
});



// add new data
$('#add-exchange-btn').click(function () {
    addExchange();
});


function addExchange() {
    fetch('http://localhost:5209/api/exchange', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            exchangeId: $("#exchange-id").val(),
            exchangeName: $("#exchange-name").val(),
            location: $("#exchange-location").val(),
            country: $("#exchange-country").val(),
            currency: $("#exchange-currency").val()
        }),
    })
    .then(response => {
        if (!response.ok) throw new Error('Failed');
        return response.json();
    })
    .then(data => {
        alert('New Exchange was added successfully!');
        console.log('Response from server:', data);
        // reset input
        document.getElementById('exchange-id').value = '';
        document.getElementById('exchange-name').value = '';
        document.getElementById('exchange-location').value = '';
        document.getElementById('exchange-country').value = '';
        document.getElementById('exchange-currency').value = '';
        showExchange();
    })
    .catch(error => {
        alert('Error: ' + error.message);
        console.error(error);
    });
}


// delete Exchange
$('#delete-exchange-btn').click(function () {
    deleteExchange();
});

function deleteExchange() {
    const exchangeIdToDelete = $("#delete-exchange-id").val();

    if (!exchangeIdToDelete) {
        alert("Please enter an Exchange ID to delete.");
        return;
    }

    fetch(`http://localhost:5209/api/exchange/${exchangeIdToDelete}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        },
    })
    .then(response => {
        if (!response.ok) {
            if (response.status === 404) {
                throw new Error(`Exchange with ID "${exchangeIdToDelete}" not found.`);
            }
            throw new Error('Failed to delete the record.');
        }
        return response.text(); // For DELETE requests, server may return a confirmation message
    })
    .then(message => {
        alert(message || 'Exchange was deleted successfully!');
        console.log('Response from server:', message);
        // clearDeleteField();
        document.getElementById('delete-exchange-id').value = '';
        showExchange(); // Optional: Refresh the display if needed
    })
    .catch(error => {
        alert('Error occurred: ' + error.message);
        console.error(error);
    });
}


// Fetch Underlying data
function fetchUnderlyingData(selectedAssets = []) {
    fetch('http://localhost:5209/api/underlying')
        .then(response => {
            if (!response.ok) throw new Error('Unable to retrieve Underlying data');
            return response.json();
        })
        .then(data => {
            let filteredData;

            if (selectedAssets.length) {
                filteredData = data.filter(item => selectedAssets.includes(item.asset));
                underlyingData = filteredData;
                console.log('filteredData:', filteredData);
            } else {
                filteredData = data;
                underlyingData = filteredData;
                console.log('filteredData:', filteredData);
            }

            renderUnderlyingTable(filteredData);
        })
        .catch(error => {
            alert('Unable to load Underlying data: ' + error.message);
            console.error(error);
        });
}


// Render Underlying table
function renderUnderlyingTable(data) {
    const tableBody = $('#underlying-data-body');
    tableBody.empty();
    if (data.length) {
        data.forEach(item => {
            tableBody.append(`
                <tr>
                    <td><input type="checkbox" class="ticker-checkbox" data-ticker="${item.ticker}"></td>
                    <td>${item.id}</td>
                    <td>${item.name}</td>
                    <td>${item.ticker}</td>
                    <td>${item.price}</td>
                    <td>${item.asset}</td>
                    <td>${item.exchangeId}</td>
                </tr>
            `);
        });

        // check the check box
        $('.ticker-checkbox').change(function () {
            selectedUnderlyings = getSelectedUnderlyings(); 
            console.log('selectedUnderlyings:', selectedUnderlyings)
        });
    } else {
        tableBody.append(`
            <tr>
                <td colspan="6" style="text-align: center;">No Data</td>
            </tr>
        `);
    }
}

// filter function
$('#filter-underlying-btn').click(function () {
    const selectedAssets = getSelectedAssets();

    // if there is selection
    if (selectedAssets.length) {
        fetchUnderlyingData(selectedAssets);
    
    // if no selection
    } else {
        fetchUnderlyingData();
    }

    // bring you to underlying page
    showUnderlying(); 
});


// reset function
$('#clear-filter-btn-underlying').click(function () {
    // clear data
    selectedAssets = [];
    console.log('Filter cleared. Selected Underlyings:', selectedAssets);
    // show full data
    fetchUnderlyingData(); 
    //showUnderlying();
});



function getSelectedAssets() {
    const selectedIds = [];
    $('.asset-checkbox:checked').each(function () {
        selectedIds.push($(this).data('id'));
    });
    return selectedIds;
}



$('#add-asset-btn').click(function () {
    addAsset();
});

function addAsset() {
    fetch('http://localhost:5209/api/asset', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            assetType: $("#asset-name").val(), 
            explanation: $("#asset-explanation").val(), 
            exchangeId: $("#asset-exchange").val() 
        }),
    })
    .then(response => {
        if (!response.ok) throw new Error('Fail');
        return response.json();
    })
    .then(data => {
        alert('New Asset was added successfully!');
        console.log('Response from server:', data);
        $("#asset-name").val('');
        $("#asset-explanation").val('');
        $("#asset-exchange").val('');
        showAsset();
    })
    .catch(error => {
        alert('Error: ' + error.message);
        console.error(error);
    });
}



$('#delete-asset-btn').click(function () {
    deleteAsset();
});

function deleteAsset() {
    const assetTypeToDelete = $("#delete-asset-id").val();

    if (!assetTypeToDelete) {
        alert("Enter Asset Type。");
        return;
    }

    fetch(`http://localhost:5209/api/asset/${assetTypeToDelete}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json',
        },
    })
    .then(response => {
        if (!response.ok) {
            if (response.status === 404) {
                throw new Error(`Asset with type "${assetTypeToDelete}" not found.`);
            }
            throw new Error('Fail。');
        }
        return response.text(); 
    })
    .then(message => {
        alert(message || 'Asset was deleted successfully!');
        console.log('Response from server:', message);
        document.getElementById('delete-asset-id').value = '';
        showAsset(); 
    })
    .catch(error => {
        alert('Error: ' + error.message);
        console.error(error);
    });
}



$('#add-underlying-btn').click(function () {
    addUnderlying();
});

function addUnderlying() {
    fetch('http://localhost:5209/api/underlying', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            name: $("#underlying-name").val(),
            ticker: $("#underlying-ticker").val(), 
            price: parseFloat($("#underlying-price").val()),
            asset: $("#underlying-asset").val(), 
            exchangeId: $("#underlying-exchange-id").val() 
        }),
    })
    .then(response => {
        if (!response.ok) throw new Error('Fail');
        return response.json();
    })
    .then(data => {
        alert('New Underlying was added successfully!');
        console.log('Response from server:', data);

        $("#underlying-name").val('');
        $("#underlying-ticker").val('');
        $("#underlying-price").val('');
        $("#underlying-asset").val('');
        $("#underlying-exchange-id").val('');

        showUnderlying();
    })
    .catch(error => {
        alert('Error: ' + error.message);
        console.error(error);
    });
}


// Delete Underlying
$('#delete-underlying-btn').click(function () {
    deleteUnderlying();
});

function deleteUnderlying() {
    const underlyingIdToDelete = $("#delete-underlying-id").val();

    if (!underlyingIdToDelete) {
        alert("Please enter an Underlying ID to delete.");
        return;
    }

    fetch(`http://localhost:5209/api/underlying/${underlyingIdToDelete}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        },
    })
    .then(response => {
        if (!response.ok) {
            if (response.status === 404) {
                throw new Error(`Underlying with ID "${underlyingIdToDelete}" not found.`);
            }
            throw new Error('Failed to delete the record.');
        }
        return response.text(); // For DELETE requests, server may return a confirmation message
    })
    .then(message => {
        alert(message || 'Underlying was deleted successfully!');
        console.log('Response from server:', message);
        document.getElementById('delete-underlying-id').value = '';
        showUnderlying(); // Optional: Refresh the display if needed
    })
    .catch(error => {
        alert('Error occurred: ' + error.message);
        console.error(error);
    });
}



function fetchDerivativeData(selectedUnderlyings = []) {
    fetch('http://localhost:5209/api/derivative')
        .then(response => {
            if (!response.ok) throw new Error('Unable to retrieve Derivate data');
            return response.json();
        })
        .then(data => {
            let filteredData;

            if (selectedUnderlyings.length) {
                filteredData = data.filter(item => selectedUnderlyings.includes(item.underlying));
                derivativeData = filteredData;

            } else {
                filteredData = data;
                derivativeData = filteredData;
            }
            console.log('filteredData:', filteredData)
            renderDerivativeTable(filteredData);
        })
        .catch(error => {
            alert('Unable to load Derivative data: ' + error.message);
            console.error(error);
        });
}


function renderDerivativeTable(data) {
    const tableBody = $('#derivative-data-body');
    tableBody.empty();

    if (data.length) {
        data.forEach(item => {
            tableBody.append(`
                <tr>
                    <td>${item.id}</td>
                    <td>${item.contractName}</td>
                    <td>${item.type}</td>
                    <td>${item.underlying}</td>
                    <td>${item.strike ?? 'N/A'}</td>
                    <td>${item.callPut ?? 'N/A'}</td>
                    <td>${item.payout ?? 'N/A'}</td>
                    <td>${item.barrier ?? 'N/A'}</td>
                    <td>${item.expiration ?? 'N/A'}</td>
                </tr>
            `);
        });

    } else {
        tableBody.append(`
            <tr>
                <td colspan="10" style="text-align: center;">No Data</td>
            </tr>
        `);
    }
}

$('#filter-derivative-btn').click(function () {
    const selectedUnderlyings = getSelectedUnderlyings();
    console.log('selectedUnderlyings:', selectedUnderlyings);
    if (selectedUnderlyings.length) {

        fetchDerivativeData(selectedUnderlyings);
    } else {

        fetchDerivativeData();
    }
    showDerivative(); 
});


function getSelectedUnderlyings() {
    const selectedIds = [];
    $('.ticker-checkbox:checked').each(function () {
        selectedIds.push($(this).data('ticker'));
    });
    return selectedIds;
}


$('#clear-filter-btn-derivative').click(function () {
    selectedUnderlyings = []; 
    console.log('Filter cleared. Selected Underlyings:', selectedUnderlyings);
    fetchDerivativeData();
    showDerivative(); 
});



$('#add-derivative-btn').click(function () {
    addDerivative();
});

function addDerivative() {
    fetch('http://localhost:5209/api/derivative', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            contractName: $("#derivative-contract-name").val(),
            type: $("#derivative-type").val(),
            underlying: $("#derivative-underlying").val(),
            strike: $("#derivative-strike").val() || null,
            callPut: $("#derivative-callput").val(),
            payout: $("#derivative-payout").val() || null,
            barrier: $("#derivative-barrier").val() || null,
            expiration: $("#derivative-expiration").val()
        }),
    })
    .then(response => {
        if (!response.ok) throw new Error('Fail');
        return response.json();
    })
    .then(data => {
        alert('New Derivative was added successfully!');
        console.log('Response from server:', data);
        document.getElementById('derivative-contract-name').value = '';
        document.getElementById('derivative-type').value = '';
        document.getElementById('derivative-underlying').value = '';
        document.getElementById('derivative-strike').value = '';
        document.getElementById('derivative-callput').value = '';
        document.getElementById('derivative-payout').value = '';
        document.getElementById('derivative-barrier').value = '';
        document.getElementById('derivative-expiration').value = '';
        showDerivatives();
    })
    .catch(error => {
        alert('Error: ' + error.message);
        console.error(error);
    });
}



function showDerivatives() {

    fetch('http://localhost:5209/api/derivative')
        .then(response => response.json())
        .then(data => {
            renderDerivativeTable(data);
        })
        .catch(error => console.error('Error fetching derivatives:', error));
}


// Delete Derivative
$('#delete-derivative-btn').click(function () {
    deleteDerivative();
});

function deleteDerivative() {
    const derivativeIdToDelete = $("#delete-derivative-id").val();

    if (!derivativeIdToDelete) {
        alert("Please enter a Derivative ID to delete.");
        return;
    }

    fetch(`http://localhost:5209/api/derivative/${derivativeIdToDelete}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        },
    })
    .then(response => {
        if (!response.ok) {
            if (response.status === 404) {
                throw new Error(`Derivative with ID "${derivativeIdToDelete}" not found.`);
            }
            throw new Error('Failed to delete the record.');
        }
        return response.text(); // For DELETE requests, server may return a confirmation message
    })
    .then(message => {
        alert(message || 'Derivative was deleted successfully!');
        console.log('Response from server:', message);
        document.getElementById('delete-derivative-id').value = '';
        showDerivatives(); // Optional: Refresh the display if needed
    })
    .catch(error => {
        alert('Error occurred: ' + error.message);
        console.error(error);
    });
}


function fetchRateCurveData() {
    fetch('http://localhost:5209/api/ratecurve')
        .then(response => {
            if (!response.ok) throw new Error('Unable to retrieve Rate data');
            return response.json();
        })
        .then(data => {
            console.log("RateCurve Data from API:", data);
            renderRateCurveTable(data);
        })
        .catch(error => {
            alert('Unable to load Rate data: ' + error.message);
            console.error(error);
        });
}


function renderRateCurveTable(data) {
    const tableBody = $('#ratecurve-data-body');
    tableBody.empty();
    if (data.length) {
        data.forEach(item => {
            tableBody.append(`
                <tr>
                    <td>${item.id || 'N/A'}</td>
                    <td>${item.maturityYear || 'N/A'}</td>
                    <td>${item.zeroCouponBonds?.toFixed(3) || 'N/A'}</td>
                    <td>${item.usTreasurys?.toFixed(2) || 'N/A'}</td>
                </tr>
            `);
        });

    } else {
        tableBody.append(`
            <tr>
                <td colspan="5" style="text-align: center;">No Data</td>
            </tr>
        `);
    }
}


function fetchTradeData() {
    fetch('http://localhost:5209/api/trade')
      .then(response => {
        if (!response.ok) throw new Error(' Unable to retrieve Trade data');
        return response.json();
      })
      .then(data => {
        console.log("Trade Data from API:", data);
        renderTradeTable(data);
        tradeData = data;
      })
      .catch(error => {
        alert('Unable to load Trade data: ' + error.message);
        console.error(error);
      });
  }


function renderTradeTable(data) {
const tableBody = $('#trade-data-body');
tableBody.empty();
if (data.length) {
    data.forEach(item => {
    tableBody.append(`
        <tr>
        <td>${item.id}</td>
        <td>${item.contractName}</td>
        <td>${item.type}</td>
        <td>${item.callPut}</td>
        <td>${item.tradeQuantity ?? 'N/A'}</td>
        <td>${item.tradePrice ?? 'N/A'}</td>
        <td>${new Date(item.tradeDate).toLocaleDateString()}</td>
        </tr>
    `);
    });
    } else {
        tableBody.append(`
        <tr>
            <td colspan="13" style="text-align: center;">No Data Found</td>
        </tr>
        `);
    }
}



$('#add-trade-btn').click(function () {
    addTrade();
});

function addTrade() {

    const tradeContractId = parseInt(document.getElementById('trade-contractid').value, 10);
    const tradeRate = parseFloat(document.getElementById('trade-rate').value);
    
    const derivativeInfo = derivativeData.find(item => item.id === tradeContractId);
    // console.log('result',result);

    // const derivativeInfo = derivativeData[tradeContractId - 1];
    // console.log(derivativeInfo.underlying);
    // console.log('indeeex',derivativeInfo);
    // console.log('indeeex',derivativeInfo.underlying);
    let index = Object.values(underlyingData).findIndex(option => option.ticker === derivativeInfo.underlying);
    
    // console.log(index);
    // console.log(underlyingData[index].price);

    fetch('http://localhost:5209/api/trade', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({

            contractName: derivativeInfo.contractName,
            type: derivativeInfo.type,
            underlying: derivativeInfo.underlying,
            strike: derivativeInfo.strike,
            callPut: derivativeInfo.callPut,
            payout: derivativeInfo.payout || 0,
            barrier: derivativeInfo.barrier || 0,
            expiration: derivativeInfo.expiration,
            stockPrice: underlyingData[index]?.price,
            tradeQuantity: $("#trade-quantity").val(),
            tradePrice: $("#trade-price").val(),
            riskFreeRate: tradeRate,
            tradeDate: $("#trade-date").val()
        }),
    })
    .then(response => {
        console.log('Raw response:', response); 
        if (!response.ok) throw new Error('Fail');
        return response.text(); 
    })
    .then(text => {
        try {
            const data = JSON.parse(text); 
            console.log('Parsed JSON:', data);
            alert('New trade was added successfully!');
            
            document.getElementById('trade-contractid').value = '';
            document.getElementById('trade-quantity').value = '';
            document.getElementById('trade-price').value = '';
            document.getElementById('trade-rate').value = '';
            document.getElementById('trade-date').value = '';

            showTrade();
        } catch (error) {
            console.error('JSON parse error:', error);
        }
    })
    .catch(error => {
        alert('Error: ' + error.message);
        console.error(error);
    });
}

// delete Trade
$('#delete-trade-btn').click(function () {
    deleteTrade();
});

function deleteTrade() {
    const tradeIdToDelete = $("#delete-trade-id").val(); 

    if (!tradeIdToDelete) {
        alert("Please enter a Trade ID to delete.");
        return;
    }

    fetch(`http://localhost:5209/api/Trade/${tradeIdToDelete}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        },
    })
    .then(response => {
        if (!response.ok) {
            if (response.status === 404) {
                throw new Error(`Trade with ID "${tradeIdToDelete}" not found.`);
            }
            throw new Error('Failed to delete the record.');
        }
        return response.text(); 
    })
    .then(message => {
        alert(message || 'Trade was deleted successfully!');
        console.log('Response from server:', message);

        document.getElementById('delete-trade-id').value = '';
        showTrade(); 
    })
    .catch(error => {
        alert('Error occurred: ' + error.message);
        console.error(error);
    });
}

async function fetchMonteCarloSimulations() { 
    try {
        const tableBody = document.getElementById('simulation-data-body');
        tableBody.innerHTML = ''; 

        let simulatePriceSum = 0; 
        let tradePriceSum = 0;

        for (let i = 0; i < tradeData.length; i++) {
            console.log(`Processing tradeData[${i}] with id:`, tradeData[i].id);

            const requestData = {
                ContractName: tradeData[i].contractName,
                Quantity: tradeData[i].tradeQuantity,
                TradePrice: tradeData[i].tradePrice,
                StockPrice: tradeData[i].stockPrice,
                StrikePrice: tradeData[i].strike,
                RiskFreeRate: tradeData[i].riskFreeRate / 100,
                StartDate: tradeData[i].tradeDate,
                EndDate: tradeData[i].expiration,
                OptionClass: tradeData[i].type,
                OptionType: tradeData[i].callPut,
                P: tradeData[i].payout,
                H: tradeData[i].barrier,
            };


            console.log("Request Data:", JSON.stringify(requestData));

            const response = await fetch('http://localhost:5209/api/montecarlo/simulate', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(requestData),
            });

            const result = await response.json();
            console.log(`Response for tradeData[${i}]:`, result);

            simulatePriceSum += result.price;
            tradePriceSum += result.tradePrice

            renderSimulationResult(result, tableBody);
        }
        renderSimulationTotal(tradePriceSum, simulatePriceSum, tableBody);
    } catch (error) {
        console.error('Error fetching Monte Carlo simulations:', error);
    }
}


function renderSimulationResult(result, tableBody) {
    const row = `
        <tr>
            <td>${result.contractName}</td>
            <td>${result.quantity}</td>
            <td>${result.tradePrice * result.quantity}</td>
            <td>${result.price.toFixed(5) * result.quantity}</td>
            <td>${result.se.toFixed(5)}</td>
            <td>${result.delta.toFixed(5)}</td>
            <td>${result.gamma.toFixed(5)}</td>
            <td>${result.vega.toFixed(5)}</td>
            <td>${result.theta.toFixed(5)}</td>
            <td>${result.rho.toFixed(5)}</td>
        </tr>
    `;
    tableBody.innerHTML += row; 
}

function renderSimulationTotal(tradePriceSum, simulatePriceSum, tableBody) {
    const totalRow = `
        <tr style="font-weight: bold; background-color: #white;">
            <td style="text-align: left;">Total:</td>
            <td> </td>
            <td>${tradePriceSum}</td>
            <td>${simulatePriceSum.toFixed(5)}</td>
            <td colspan="6"></td>
        </tr>
    `;
    tableBody.innerHTML += totalRow;
}

document.getElementById('evaluate-trade').addEventListener('click', fetchMonteCarloSimulations);
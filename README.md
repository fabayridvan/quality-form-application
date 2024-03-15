<head>
<!-- Select2 CSS -->
<script src="https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.13/js/select2.min.js"></script>
 
    <link href="https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.13/css/select2.min.css" rel="stylesheet" />
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            padding: 20px;
            justify-content: center; /* Center content horizontally */
            align-items: center; /* Center content vertically */
            min-height: 100vh; /* Full viewport height */
        }
 
        form {
            background-image: url('https://fabitech.com.tr/assets/media/logos/constructor-form.png'); /* Arka plan resmi */
            background-color: rgba(255, 255, 255, 0.3); /* Beyaz renk ve yarı şeffaf arka plan */
            background-blend-mode: color-dodge; /* Arka plan resmi ve rengi arasında karışım modu */
            background-size: cover; /* Arka plan resmini kaplayacak şekilde ayarla */
            background-position: center; /* Arka plan resmini merkeze al */
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            display: flex;
            flex-wrap: wrap;
            gap: 20px;
/*          margin: auto; /* Sayfanın ortasına yerleştirir */*/         
            width: fit-content; /* İçeriğin genişliğine göre ayarlar */
            justify-content: center; /* Yatay eksende ortalar */
        }
 
            form > div {
                flex: 1;
                min-width: 100%; /* min-width ekleyerek yan yana sığabilecek minimum genişliği belirleyin */
                flex-direction: column;
                align-items: center;
            }
 
        h1 {
            color: #333333;
            margin-bottom: 0.5em;
            text-align: center; /* Add this line to center the text */
        }
 
        h3 {
            color: #333333;
            margin-bottom: 0.5em;
            align-items:center;
        }
 
        select {
            width: 100%;
            padding: 10px;
            margin-bottom: 20px;
            border-radius: 4px;
            border: 1px solid #cccccc;
            background-color: rgba(255, 255, 255, 0.7);
        }
 
        button {
            background-color: #4CAF50;
            color: white;
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
        }
 
            button:hover {
                background-color: #45a049;
            }
        /* New styles for the inline radio buttons */
        .radio-options {
            display: flex;
        }
 
        /* New styles for question list */
        .question-list {
            background-color: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            margin-top: 20px;
            display: flex;
            flex-direction: column;
        }
 
            .question-list ul {
                list-style-type: none;
                padding: 0;
            }
 
            .question-list li {
                margin-bottom: 10px;
            }
</style>
</head>
 
<body>
<form method="post">
<h1>Kalite Kontrol Anketi</h1>
<div class="col-12">
<div class="col-12">
<div>
<h3>Firma</h3>
<select id="companySelect" asp-items="@(new SelectList(Model.Companies, "Id", "CoCode"))">
    <option value="">Şirket seçin...</option>
</select>
</div>
 
            <div>
<h3>Kategori-1</h3>
<select id="urunSelect2" asp-items="@(new SelectList(Model, "Id", "CoCode"))">
<option value="">Lütfen bir kategori seçin...</option>
</select>
</div>
 
            <div>
<h3>Kategori-2</h3>
<select id="urunSelect3" asp-items="@(new SelectList(Model, "Id", "CoCode"))">
<option value="">Lütfen bir kategori seçin...</option>
</select>
</div>
 
            <div>
<h3>İmalat Grubu</h3>
<select id="urunSelect4" asp-items="@(new SelectList(Model, "Id", "CoCode"))">
<option value="">Lütfen bir imalat grubu seçin...</option>
</select>
</div>
 
            <div>
<h3>İmalat Adı</h3>
<select id="urunSelect5" asp-items="@(new SelectList(Model, "Id", "CoCode"))">
<option value="">Lütfen bir İmalat adı seçin...</option>
</select>
</div>
<div style="flex-basis: 100%;">
<button type="button" class="bg-primary">Soruları Listele</button>
</div>
 
            <!-- Question list section, initially hidden -->
<div class="question-list" id="questionList" style="display:none;">
<h3>Sorular Listesi</h3>
<ul id="questionItems">
<!-- Question items will be listed here -->
</ul>
<button type="button" onclick="submitForm()" class="save-button">Anketi Kaydet</button>
</div>
</div>
</div>

</form>
 
 
    <!-- Select2 JS and additional scripts -->
<script src="https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.13/js/select2.min.js"></script>
<script>
        // Function to create radio buttons
        function createRadioButtons(name) {
            const div = document.createElement('div');
            div.className = 'radio-options';
 
            const accept = document.createElement('input');
            accept.type = 'radio';
            accept.name = name;
            accept.value = 'accept';
 
            div.appendChild(accept);
            div.appendChild(document.createTextNode('Kabul'));
 
            const reject = document.createElement('input');
            reject.style.marginLeft = '10px'; // Add margin to the right of the radio button
 
            reject.type = 'radio';
            reject.name = name;
            reject.value = 'reject';
            div.appendChild(reject);
            div.appendChild(document.createTextNode('Red'));
 
            return div;
        }
 
        // Handling the listing of questions
        document.querySelector('.bg-primary').addEventListener('click', function () {
            var selectElements = document.querySelectorAll('select');
            var list = document.getElementById('questionItems');
            list.innerHTML = ''; // Clear existing list items
            selectElements.forEach(function (select, index) {
                var optionText = select.options[select.selectedIndex].text;
                if (optionText) {
                    var li = document.createElement('li');
                    li.textContent = optionText;
 
                    // Append radio buttons to each list item
                    var radios = createRadioButtons('option' + index);
                    li.appendChild(radios);
 
                    list.appendChild(li);
                }
            });
            document.getElementById('questionList').style.display = 'flex'; // Show question list
        });
 
        // Function to handle form submission
        function submitForm() {
            // Add your form submission logic here
            alert('Anket Kaydedildi');
        }
</script>
</body># Quality-Form-App

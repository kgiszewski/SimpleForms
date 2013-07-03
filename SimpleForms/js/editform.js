$(function(){

    var $actionBox=$(".actionBox");
    var $fieldsTable=$(".fieldsTable");
    
    $(".formFields").click(function(){
        $fieldsTable.toggle();
    });

    $('.fieldsTable tbody').sortable({
        handle: ".sortFieldsHandle",
        cursor: 'move',
        update: function(){buildJSON();},
        start : function(e, ui){ui.placeholder.html('<div>Insert Here</div>')}
        //placeholder: 'widgetElementPlaceholder'
    });
    
    $('.add').on('click', function(){
        var $button=$(this);
        var $tr=$button.closest('tr');
        
        var $newRow=$tr.clone(true, true);
        $tr.after($newRow);
        
        //clear boxes and fieldID
        $newRow.attr('fieldID', '0');
        $newRow.find('input').val('');
        buildJSON();
    });
    
    $('.remove').click(function(){
        //check to make sure this isn't the last one
        var $button=$(this);
        var $tbody=$button.closest('tbody');
        var $tr=$button.closest('tr');
        
        if($tbody.find('tr').length>1){
            if(confirm('Are you sure you wish to remove this?  Only fields that have no data submissions can be deleted.  If this field contains data in the database, the removal will fail.')){
                $tr.hide();
                buildJSON();
            }
        }
    });
    
    $('.unpublish').click(function(){
        $actionBox.val("0");
    });
    
    $('.save').click(function(){
        $actionBox.val("1");
    });
    
    $('.savePublish').click(function(){
        $actionBox.val("2");
    });

    $('.preview').click(function(){
        window.open('/umbraco/dialogs/preview.aspx?id='+$(this).attr('docID'), '_blank');
    });
    
    $('.fieldsTable input').keyup(function(){
        buildJSON();
    });
    
    $(".search").click(function(){
        getSubmissions();
    });
    
    $(document).keypress(function(e) {
      if(e.which == 13) {
        $('.search').click();
        e.preventDefault();
      }
    });
    
    $(".download").click(function(){
        window.location.href="/umbraco/plugins/SimpleForms/download.aspx?keywords="+escape($(".keywords").val())+"&maxResults="+$(".maxResults").val()+"&formAlias="+$(".fieldsTable").attr('formAlias')+"&occurring="+$(".occurring").val();
    });
    
    //ini
    buildJSON();
    stripe();
    
    //click the search
    $('.search').click();
    
    //turn on iframe scrolling
    var iframe=parent.document.getElementById('right');
    $(iframe).attr('scrolling', 'yes');
});

function buildJSON(){
    
    var $saveBox=$('.saveBox');
    var $fieldsTable=$('.fieldsTable');
    var json="[";
    var fields=[];
    
    $fieldsTable.find('tbody tr').each(function(sortOrder){
        
        var $tr=$(this);
        var $inputs=$tr.find('input');
        
        fields.push(buildFieldJSON($tr.attr('fieldID'), $($inputs[0]).val(), $($inputs[1]).val(), sortOrder, !$tr.is(":visible")));
    
    });
    
    json+=fields.join(',');
    
    json+="]";
    
    $saveBox.val(json);
}

function buildFieldJSON(ID, name, alias, sortOrder, remove){
    return "{\"id\":\""+ID+"\", \"name\":\""+escape(name)+"\", \"alias\":\""+escape(alias)+"\", \"sortOrder\":\""+sortOrder+"\", \"remove\":\""+remove+"\"}";
}

function escape(string){
    return (encodeURI(string));
}

function getSubmissions(){
  $.ajax({
      type: "POST",
      async: false,
      url: "/umbraco/plugins/SimpleForms/SimpleFormsWebService.asmx/GetSubmissions",
      data: '{"keywords":"'+escape($(".keywords").val())+'", "maxResults":'+$(".maxResults").val()+', "formAlias":"'+$(".fieldsTable").attr('formAlias')+'", "occurring":"'+$(".occurring").val()+'"}',
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      success: function (returnValue){
        var response=returnValue.d;
        //console.log(response);
        
        switch(response.status){
          case 'SUCCESS':
              updateResults(eval(response.entries));
              break;
          case 'ERROR':
              var $tbody=$('.resultsDiv tbody');
              $tbody.html('');
              
              $tbody.append("<tr><td>"+response.message+"</td></tr>");
              break;
        }
      }
  });
}

function deleteSubmission($tr){
  $.ajax({
      type: "POST",
      async: false,
      url: "/umbraco/plugins/SimpleForms/SimpleFormsWebService.asmx/DeleteSubmission",
      data: '{"submissionID":"'+$tr.attr('submissionID')+'"}',
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      success: function (returnValue){
        var response=returnValue.d;
        //console.log(response);
        
        switch(response.status){
          case 'SUCCESS':
              $tr.hide();
              break;
          case 'ERROR':
              alert(response.message);
              break;
        }
      }
  });
}

function updateResults(entries){
    var $thead=$('.resultsDiv thead');
    var numCols=$thead.find('th').length;
    
    var $tbody=$('.resultsDiv tbody');
    $tbody.find('tr').remove();
    
    if(entries.length>0){
        
        for(var i=0;i<entries.length;i++){
            var $newTR=$("<tr submissionID='"+entries[i].ID+"'></tr>");
            $tbody.append($newTR);
            
            var $newTD;
            
            $newTD=$("<td></td>");
            $newTR.append($newTD);
            $newTD.html(entries[i].dateTime);
            
            $newTD=$("<td></td>");
            $newTR.append($newTD);
            $newTD.html(entries[i].IP);
            
            for(var j=0;j<entries[i].values.length;j++){
                $newTD=$("<td></td>");
                $newTR.append($newTD);
                $newTD.html(entries[i].values[j]);
            }
            
            $newTD=$("<td><img class='delete' src='/umbraco/plugins/SimpleForms/images/minus.png'/></td>");
            $newTR.append($newTD);
        }
        
        $('.delete').click(function(){
            
            var $button=$(this);
            var $tbody=$button.closest('tbody');
            var $tr=$button.closest('tr');
            
            if(confirm('Are you sure you wish to remove this?  This cannot be undone.')){
                deleteSubmission($tr);
            }
        });
        
    } else {
        var $newTR=$("<tr></tr>");
        $tbody.append($newTR);
        
        var $newTD=$("<td colspan='"+numCols+"'>No results found.</td>");
        $newTR.append($newTD);
    }
    
    stripe();
}

function stripe(){
  $(".resultsDiv tr").not(':first').hover( 
    function () {
      $(this).addClass('rowHighlight');
    }, 
    function () {
      $(this).removeClass('rowHighlight');
    }
  );
}



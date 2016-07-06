<?php
  
class Rf {
  public $r;
  function __construct($value) {
    $this->r = $value;
  }
}
  
function pth_noop() { }
  
function pth_new_array($size) {
  $output = array();
  while ($size-- > 0) array_push($output, null);
  return new Rf($output);
}
  
function pth_dictionary_get_keys($dictionary) {
  $output = array();
  foreach ($dictionary->r as $key => $ignored) {
    array_push($output, $key);
  }
  return new Rf($output);
}
  
$pth_program_data = null;
function pth_setProgramData($pd) {
  $GLOBALS['pth_program_data'] = $pd;
}
  
function pth_getProgramData() {
  return $GLOBALS['pth_program_data'];
}
  
function pth_list_reverse($list) {
  $length = count($list->r);
  for ($i = intval($length / 2) - 1; $i >= 0; --$i) {
    $j = $length - $i - 1;
    $t = $list->r[$i];
    $list->r[$i] = $list->r[$j];
    $list->r[$j] = $t;
  }
}
  
function pth_getHttpRequest($stringOut) {
  $stringOut->r[0] = $_SERVER['REQUEST_METHOD'];
  $stringOut->r[1] = $_SERVER['REQUEST_URI'];
  $stringOut->r[2] = $_SERVER['REMOTE_HOST'];
}

function pth_mysql_connect($errorOut, $host, $user, $pass, $db) {
  $error = null;
  $link = mysqli_connect($host, $user, $pass, $db) or $error = "Could not connect to MySQL.";
  $errorOut->r[0] = $error;
  return $error == null ? $link : null;
}

function pth_mysql_query($db, $query, $stringOut, $intOut, $columnsOut, $typesOut) {
  $result = $db->query($query);

  if ($db->errno !== 0) {
    $error = $db->error;
  } else {
    $error = null;
    // TODO: updated rows as well.
    $intOut->r[0] = $db->insert_id;
  
	  $fields = $result->fetch_fields();
    
	  for ($i = 0; $i < count($fields); ++$i) {
	    $field = $fields[$i];
	    array_push($columnsOut->r, $field->name);
      // TODO: a better job. In the interest of prototyping, I categorized these value types to the closest crayon counterpart.
      // These values are described here: http://php.net/manual/en/mysqli-result.fetch-fields.php
	    switch ($field->type) {
	    
	      // boolean-likes
	      case 1:
	      case 16:
	        array_push($typesOut->r, 0);
	        break;
	      
	      // int-likes
	      case 2:
	      case 3:
	      case 8:
	      case 9:
	        array_push($typesOut->r, 1);
	        break;
	      
	      // float-likes
	      case 4:
	      case 5:
	      case 246:
	        array_push($typesOut->r, 2);
	        break;
	      
	      // string-likes
	      case 7:
	      case 10:
	      case 11:
	      case 12:
	      case 13:
	      case 252:
	      case 253:
	      case 254:
		      array_push($typesOut->r, 3);
		      break;
	    }
    }
	}
  $stringOut->r[0] = $error;
  return $result;
}

function pth_mysql_num_rows($result) {
  return $result->num_rows;
}

?>
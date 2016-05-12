var $apiKeyInput = $('#input_apiKey');
$apiKeyInput.change(function() {
    var bearer = $apiKeyInput.val();
    window.swaggerUi.api.clientAuthorizations.add("bearer", new SwaggerClient.ApiKeyAuthorization("Authorization", 'bearer ' + bearer, "header"));
    //window.authorizations.add('key', new ApiKeyAuthorization('Authorization', 'bearer ' + bearer, 'header'));
});